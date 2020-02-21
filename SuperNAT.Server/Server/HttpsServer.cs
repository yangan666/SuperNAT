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
    public class HttpsServer : SocketServer<TcpSession, HttpRequestInfo>
    {
        public NatServer NATServer { get; set; }
        public HttpsServer(ServerOption serverOption) : base(serverOption)
        {
            ReceiveFilter = new HttpReceiveFilter();
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
            HandleLog.WriteLine($"HTTP客户端【{session.SessionId},{session.Remote}】已连接【{session.Local}】");
        }

        private void Received(TcpSession session, HttpRequestInfo requestInfo)
        {
            Task.Run(() =>
            {
                try
                {
                    var httpModel = new HttpModel
                    {
                        RequestTime = DateTime.Now,
                        ServerId = ServerId,
                        HttpVersion = requestInfo.HttpVersion,
                        Method = requestInfo.Method,
                        Path = requestInfo.Path,
                        Headers = requestInfo.Headers,
                        Host = requestInfo.Headers["Host"],
                        Content = requestInfo.ContentLength > 0 ? DataHelper.Compress(requestInfo.Body) : null
                    };
                    //转发请求
                    var natSession = NATServer.GetSingle(c => c.MapList.Any(c => c.remote_endpoint == httpModel.Host));
                    if (natSession == null)
                    {
                        //TODO 错误页面
                        HandleLog.WriteLine($"穿透客户端未连接到服务器，请求地址：{httpModel.Host}{httpModel.Path}");
                        var response = new HttpResponse()
                        {
                            Status = 404,
                            Body = "nat client not found"
                        };
                        //把处理信息返回到客户端
                        session.Send(response.Write404());
                    }
                    else
                    {
                        //转发数据
                        var pack = new JsonData()
                        {
                            Type = (int)JsonType.HTTP,
                            Action = (int)HttpAction.Request,
                            Data = httpModel.ToJson()
                        };
                        session.NatSession.Send(PackHelper.CreatePack(pack));
                    }
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
                HandleLog.WriteLine($"HTTP客户端【{session.SessionId},{session.Remote}】已下线");
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"关闭连接【{session.Local}】发生异常：{ex}");
            }
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
