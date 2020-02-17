using SuperNAT.AsyncSocket;
using SuperNAT.Bll;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperNAT.Server
{
    public class NatServer : SocketServer<NatSession, NatRequestInfo>
    {
        public NatServer(ServerOption serverOption) : base(serverOption)
        {
            ReceiveFilter = new NatReceiveFilter();
        }

        public void SendServerMessage(NatSession session, ServerMessage serverMessage)
        {
            HandleLog.WriteLine(serverMessage.Message);
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
                            var bll = new ClientBll();
                            var client = bll.GetOne(secret).Data;
                            if (client == null)
                            {
                                HandleLog.WriteLine($"主机【{session.Remote}】密钥不正确！！");
                                SendServerMessage(session, new ServerMessage() { Message = "主机密钥不正确，请确认是否填写正确！" });
                                return;
                            }
                            var checkSessions = GetList(c => c.Client?.secret == secret && c.SessionId != session.SessionId);
                            if (checkSessions.Any())
                            {
                                checkSessions.ForEach(c =>
                                {
                                    SendServerMessage(c, new ServerMessage() { Message = "该主机密钥已被其它主机使用，您已被迫下线！" });
                                    Thread.Sleep(500);
                                    c.Close();
                                });
                            }
                            session.Client = client;

                            var mapBll = new MapBll();
                            session.MapList = mapBll.GetMapList(secret).Data ?? new List<Map>();
                            //原样返回回复客户端注册成功
                            session.Send(requestInfo.Raw);
                            Task.Run(() =>
                            {
                                //更新在线状态
                                var bll = new ClientBll();
                                var updateRst = bll.UpdateOnlineStatus(new Client() { secret = session.Client.secret, is_online = true, last_heart_time = DateTime.Now });
                                HandleLog.WriteLine($"更新主机【{session.Client.name}】在线状态结果：{updateRst.Message}", false);
                            });
                        }
                        break;
                    case (int)NatAction.Heart:
                        {
                            //心跳包
                            var secret = requestInfo.Body.Data.ToString();
                            HandleLog.WriteLine($"收到连接{session.Remote}的心跳包，密钥为：{secret}，当前映射个数：{session.MapList.Count}", false);
                            Task.Run(() =>
                            {
                                //更新在线状态
                                var bll = new ClientBll();
                                var updateRst = bll.UpdateOnlineStatus(new Client() { secret = session.Client.secret, is_online = true, last_heart_time = DateTime.Now });
                                HandleLog.WriteLine($"更新主机【{session.Client.name}】在线状态结果：{updateRst.Message}", false);
                            });
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"穿透处理异常，{ex}");
            }
        }
    }
}
