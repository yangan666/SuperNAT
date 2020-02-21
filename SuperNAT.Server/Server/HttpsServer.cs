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
                        session.Send(response.Write());
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

        public void ProcessData(NatSession session, NatRequestInfo requestInfo, HttpModel httpModel)
        {
            try
            {
                switch (requestInfo.Body.Action)
                {
                    case (int)HttpAction.Response:
                        {
                            var context = GetSingle(c => c.SessionId == httpModel.SessionId);
                            if (context == null)
                            {
                                HandleLog.WriteLine($"未找到上下文context，SessionId={httpModel.SessionId}");
                                return;
                            }
                            HttpResponse httpResponse = new HttpResponse()
                            {
                                HttpVersion = httpModel.HttpVersion,
                                Status = httpModel.StatusCode,
                                StatusMessage = httpModel.StatusMessage
                            };
                            if (httpModel.Content?.Length > 0)
                            {
                                //解压
                                var byteData = DataHelper.Decompress(httpModel.Content);
                                httpResponse.Body = byteData.ToUTF8String();
                                //把处理信息返回到客户端
                                context.Send(byteData);

                                var timeSpan = (DateTime.Now - httpModel.RequestTime);
                                var totalSize = byteData.Length * 1.00 / 1024;
                                var speed = Math.Round(totalSize / timeSpan.TotalSeconds, 1);
                                HandleLog.WriteLine($"{session.Client.user_name} {session.Client.name} {httpModel.Method} {httpModel.Host}{httpModel.Path} {httpModel.StatusCode} {httpModel.StatusMessage} {Math.Round(totalSize, 1)}KB {timeSpan.TotalMilliseconds}ms {speed}KB/s");

                                //关闭http连接
                                context.Close();
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"HttpsServer ProcessData穿透处理异常，{ex}");
            }
        }
    }
}
