using SuperNAT.AsyncSocket;
using SuperNAT.Bll;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperNAT.Core
{
    public class NatServer : AppServer<NatSession, NatRequestInfo>
    {
        public NatServer(ServerOption serverOption) : base(new NatReceiveFilter())
        {
            InitOption(serverOption);
        }

        public void SendServerMessage(NatSession session, ServerMessage serverMessage)
        {
            HandleLog.Log(serverMessage.Message);
            var pack = new JsonData()
            {
                Type = (int)JsonType.NAT,
                Action = (int)NatAction.ServerMessage,
                Data = serverMessage.ToJson()
            };
            //转发给客户端
            session?.Send(PackHelper.CreatePack(pack));
        }

        public void ProcessData(NatSession session, NatRequestInfo requestInfo)
        {
            try
            {
                switch (requestInfo.Body.Action)
                {
                    case (int)NatAction.Connect:
                        {
                            //注册包
                            var secret = requestInfo.Body.Data.ToString();
                            HandleLog.Log($"收到连接{session.RemouteEndPoint}的注册包，密钥为：{secret}，当前映射个数：{session.MapList.Count}", false);
                            var bll = new ClientBll();
                            var client = bll.GetOne(secret).Data;
                            if (client == null)
                            {
                                HandleLog.Log($"主机【{session.RemouteEndPoint}】密钥不正确！！");
                                SendServerMessage(session, new ServerMessage() { Message = "主机密钥不正确，请确认是否填写正确！" });
                                return;
                            }
                            var checkSessions = GetSessionList(c => c.Client?.secret == secret && c.SessionId != session.SessionId);
                            if (checkSessions.Any())
                            {
                                checkSessions.ForEach(c =>
                                {
                                    SendServerMessage(c, new ServerMessage() { Message = "该主机密钥已被其它主机使用，您已被迫下线！" });
                                    Thread.Sleep(500);
                                    c.Close();
                                });
                            }

                            var mapBll = new MapBll();
                            session.MapList = mapBll.GetMapList(secret).Data ?? new List<Map>();
                            client.MapList = session.MapList;
                            session.Client = client;
                            //原样返回回复客户端注册成功
                            requestInfo.Body.Data = client.ToJson();
                            session.Send(PackHelper.CreatePack(requestInfo.Body));
                            Task.Run(() =>
                            {
                                //更新在线状态
                                var bll = new ClientBll();
                                var updateRst = bll.UpdateOnlineStatus(new Client() { secret = session.Client.secret, is_online = true, last_heart_time = DateTime.Now });
                                HandleLog.Log($"更新主机【{session.Client.name}】在线状态结果：{updateRst.Message}", false);
                            });
                        }
                        break;
                    case (int)NatAction.Heart:
                        {
                            //心跳包
                            var secret = requestInfo.Body.Data.ToString();
                            HandleLog.Log($"收到连接{session.RemouteEndPoint}的心跳包，密钥为：{secret}，当前映射个数：{session.MapList.Count}", false);
                            Task.Run(() =>
                            {
                                //更新在线状态
                                var bll = new ClientBll();
                                var updateRst = bll.UpdateOnlineStatus(new Client() { secret = session.Client.secret, is_online = true, last_heart_time = DateTime.Now });
                                HandleLog.Log($"更新主机【{session.Client.name}】在线状态结果：{updateRst.Message}", false);
                            });
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                HandleLog.Log($"穿透处理异常，{ex}");
            }
        }
    }
}
