using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperNAT.Server
{
    public class TcpServer : SocketServer<TcpSession, NatRequestInfo>
    {
        public NatServer NATServer { get; set; }
        public TcpServer(ServerOption serverOption) : base(serverOption)
        {

        }

        public override async Task StartAsync()
        {
            await Task.Run(async () =>
            {
                OnConnected += Connected;
                OnReceived += Received;
                OnClosed += Closed;
                await base.StartAsync();
            });
        }

        private void Connected(TcpSession session)
        {
            try
            {
                //转发连接请求
                var natSession = NATServer.GetSingle(c => c.MapList?.Any(m => m.remote_port == ServerOption.Port) ?? false);
                if (natSession == null)
                {
                    session?.Close();
                    HandleLog.WriteLine($"请求：{session.Local}失败，Nat客户端连接不在线！");
                    return;
                }
                var map = natSession.MapList?.Find(c => c.remote_port == ServerOption.Port);
                if (map == null)
                {
                    session?.Close();
                    HandleLog.WriteLine($"请求：{session.Local}失败，映射{session.Local}不存在！");
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
                HandleLog.WriteLine($"客户端【{session.SessionId},{session.Remote}】已连接【{session.Local}】");
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"连接【{session.SessionId},{session.Local}】发生异常：{ex}");
            }
        }

        private void Received(TcpSession session, NatRequestInfo requestInfo)
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
                    HandleLog.WriteLine($"<---- {session.SessionId} 收到报文{body.Length}字节");
                }
                catch (Exception ex)
                {
                    HandleLog.WriteLine($"【{session.Local}】请求参数：{requestInfo.Raw.ToHexWithSpace()}，处理发生异常：{ex}");
                }
            });
        }

        private void Closed(TcpSession session)
        {
            try
            {
                CloseLocalClient(session);
                HandleLog.WriteLine($"客户端【{session.SessionId},{session.Remote}】已下线");
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"关闭连接【{session.Local}】发生异常：{ex}");
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
                            var tcpSession = GetSingle(c => c.SessionId == tcpModel.SessionId);
                            if (tcpSession == null)
                            {
                                count++;
                                Thread.Sleep(500);
                                if (count < 5)
                                {
                                    goto mark;
                                }
                                HandleLog.WriteLine($"tcpSession【{tcpModel.SessionId}】不存在");
                                return;
                            }
                            //先讲16进制字符串转为byte数组  再gzip解压
                            var response = DataHelper.Decompress(tcpModel.Content);
                            tcpSession.Send(response);
                            HandleLog.WriteLine($"----> {tcpSession.SessionId} 发送报文{response.Length}字节");
                        }
                        break;
                    case (int)TcpAction.Close:
                        {
                            //关闭请求
                            var tcpSession = GetSingle(c => c.SessionId == tcpModel.SessionId);
                            if (tcpSession != null)
                            {
                                tcpSession.Close();
                                HandleLog.WriteLine($"连接【{tcpSession.SessionId},{tcpSession.Remote}】关闭成功");
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"TcpServer穿透处理异常，{ex}");
            }
        }
    }
}
