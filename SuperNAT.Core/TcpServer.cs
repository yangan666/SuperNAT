using SuperNAT.AsyncSocket;
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
    public class TcpServer : AppServer<TcpSession, TcpRequestInfo>
    {
        public TcpServer(ServerOption serverOption) : base(null)//不需要过滤器，原样转发
        {
            InitOption(serverOption);
            OnConnected += Connected;
            OnReceived += Received;
            OnClosed += Closed;
        }

        private void Connected(TcpSession session)
        {
            try
            {
                //转发连接请求
                var natSession = ServerManager.NATServer.GetSingleSession(c => c.MapList?.Any(m => m.remote_port == ServerOption.Port) ?? false);
                if (natSession == null)
                {
                    session?.Close();
                    HandleLog.Log($"请求：{session.LocalEndPoint}失败，Nat客户端连接不在线！");
                    return;
                }
                var map = natSession.MapList?.Find(c => c.remote_port == ServerOption.Port);
                if (map == null)
                {
                    session?.Close();
                    HandleLog.Log($"请求：{session.LocalEndPoint}失败，映射{session.LocalEndPoint}不存在！");
                    return;
                }
                session.Map = map;
                //tcp连接注册包
                var pack = new JsonData()
                {
                    Type = (int)JsonType.TCP,
                    Action = (int)TcpAction.Connect,
                    Data = new TcpModel()
                    {
                        ServerId = ServerId,
                        Host = map?.remote_endpoint,
                        Local = map?.local_endpoint,
                        SessionId = session.SessionId
                    }.ToJson()
                };
                natSession.Send(PackHelper.CreatePack(pack));

                session.NatSession = natSession;
                HandleLog.Log($"{session.Map.name} {session.Map.protocol} {session.Map.remote_endpoint} --> {session.Map.local_endpoint} 客户端【{session.RemouteEndPoint}】已连接到服务器");
            }
            catch (Exception ex)
            {
                HandleLog.Log($"连接【{session.SessionId},{session.RemouteEndPoint},{session.LocalEndPoint}】发生异常：{ex}");
            }
        }

        private void Received(TcpSession session, TcpRequestInfo requestInfo)
        {
            Task.Run(() =>
            {
                try
                {
                    while (session.NatSession == null)
                    {
                        Thread.Sleep(50);
                    }
                    //先gzip压缩  再转为16进制字符串
                    var body = DataHelper.Compress(requestInfo.Raw);
                    //转发数据
                    var pack = new JsonData()
                    {
                        Type = (int)JsonType.TCP,
                        Action = (int)TcpAction.TransferData,
                        Data = new TcpModel()
                        {
                            ServerId = ServerId,
                            Host = session.Map?.remote_endpoint,
                            Local = session.Map?.local_endpoint,
                            SessionId = session.SessionId,
                            Content = body
                        }.ToJson()
                    };
                    session.NatSession.Send(PackHelper.CreatePack(pack));
                    HandleLog.Log($"{session.Map.name} {session.Map.protocol} {session.Map.remote_endpoint} --> {session.Map.local_endpoint} 收到报文{body.Length}字节");
                }
                catch (Exception ex)
                {
                    HandleLog.Log($"{session.Map.name} {session.Map.protocol} {session.Map.remote_endpoint} --> {session.Map.local_endpoint} 请求参数：{requestInfo.Raw.ToHexWithSpace()}，处理发生异常：{ex}");
                }
            });
        }

        private void Closed(TcpSession session)
        {
            try
            {
                CloseLocalClient(session);
                HandleLog.Log($"{session.Map.name} {session.Map.protocol} {session.Map.remote_endpoint} --> {session.Map.local_endpoint} 客户端【{session.RemouteEndPoint}】已下线");
            }
            catch (Exception ex)
            {
                HandleLog.Log($"{session.Map.name} {session.Map.protocol} {session.Map.remote_endpoint} --> {session.Map.local_endpoint} 关闭连接【{session.LocalEndPoint}】发生异常：{ex}");
            }
        }

        public void CloseLocalClient(TcpSession session)
        {
            var pack = new JsonData()
            {
                Type = (int)JsonType.TCP,
                Action = (int)TcpAction.Close,
                Data = new TcpModel()
                {
                    ServerId = ServerId,
                    Host = session.Map?.remote_endpoint,
                    Local = session.Map?.local_endpoint,
                    SessionId = session.SessionId
                }.ToJson()
            };
            //转发给客户端
            session.NatSession?.Send(PackHelper.CreatePack(pack));
        }

        public void ProcessData(NatSession session, NatRequestInfo requestInfo, TcpModel tcpModel)
        {
            try
            {
                switch (requestInfo.Body.Action)
                {
                    case (int)TcpAction.TransferData:
                        {
                            //响应请求
                            int count = 0;
                            mark:
                            var tcpSession = GetSingleSession(c => c.SessionId == tcpModel.SessionId);
                            if (tcpSession == null)
                            {
                                count++;
                                Thread.Sleep(500);
                                if (count < 5)
                                {
                                    goto mark;
                                }
                                HandleLog.Log($"tcpSession【{tcpModel.SessionId}】不存在");
                                return;
                            }
                            //先讲16进制字符串转为byte数组  再gzip解压
                            var response = DataHelper.Decompress(tcpModel.Content);
                            tcpSession.Send(response);
                            HandleLog.Log($"{tcpSession.Map.name} {tcpSession.Map.protocol} {tcpSession.Map.remote_endpoint} --> {tcpSession.Map.local_endpoint} 发送报文{response.Length}字节");
                        }
                        break;
                    case (int)TcpAction.Close:
                        {
                            //关闭请求
                            var tcpSession = GetSingleSession(c => c.SessionId == tcpModel.SessionId);
                            if (tcpSession != null)
                            {
                                tcpSession.Close();
                                HandleLog.Log($"连接【{tcpSession.SessionId},{tcpSession.RemouteEndPoint}】关闭成功");
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                HandleLog.Log($"TcpServer穿透处理异常，{ex}");
            }
        }
    }
}