using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Server
{
    public class HttpServer
    {
        public NatServer NATServer { get; set; }
        public HttpListener HttpListener { get; set; }
        public string ServerId { get; set; } = Guid.NewGuid().ToString();
        public string Url { get; set; }
        public Dictionary<string, HttpListenerContext> ContextDict { get; set; } = new Dictionary<string, HttpListenerContext>();

        public HttpServer(int port)
        {
            Url = $"http://+:{port}/";
        }

        public void Start()
        {
            HttpListener = new HttpListener();
            HttpListener.Prefixes.Add(Url);
            HttpListener.Start();

            HttpListener.BeginGetContext(OnHandleContext, HttpListener);
        }

        private void OnHandleContext(IAsyncResult ar)
        {
            try
            {
                //继续异步监听下一次请求
                HttpListener.BeginGetContext(OnHandleContext, HttpListener);

                //当前请求上下文
                var context = HttpListener.EndGetContext(ar);
                //转发请求
                var natSession = NATServer.GetSingle(c => c.MapList.Any(c => c.remote_endpoint == context.Request.Url.Authority));
                if (natSession == null)
                {
                    //TODO 错误页面
                    HandleLog.WriteLine($"穿透客户端未连接到服务器，请求地址：{context.Request.Url.AbsoluteUri}");
                    context.Response.StatusCode = 404;
                    var returnByteArr = Encoding.UTF8.GetBytes("nat client not found");
                    using var stream = context.Response.OutputStream;
                    //把处理信息返回到客户端
                    stream.WriteAsync(returnByteArr, 0, returnByteArr.Length);
                }
                else
                {
                    var sessionId = Guid.NewGuid().ToString();
                    ContextDict.Add(sessionId, context);

                    var map = natSession.MapList.Find(c => c.remote_endpoint == context.Request.Url.Authority);
                    var httpModel = new HttpModel()
                    {
                        RequestTime = DateTime.Now,
                        ServerId = ServerId,
                        HttpVersion = $"{map?.protocol.ToUpper()}/{context.Request.ProtocolVersion.ToString()}",
                        Host = context.Request.Url.Authority,
                        SessionId = sessionId,
                        Method = context.Request.HttpMethod,
                        Path = context.Request.RawUrl,
                        Headers = context.Request.Headers.ToDictionary(),
                        ContentType = context.Request.ContentType
                    };

                    var byteList = new List<byte>();
                    int readLen = 0;
                    int len = 0;
                    if (context.Request.HttpMethod != "Get")
                    {
                        var byteArr = new byte[2048];
                        do
                        {
                            readLen = context.Request.InputStream.Read(byteArr, 0, byteArr.Length);
                            len += readLen;
                            byteList.AddRange(byteArr);
                        } while (readLen != 0);

                        var byteData = byteList.CloneRange(0, len);
                        var req = byteData.ToUTF8String();

                        if (byteData.Length > 0)
                        {
                            //压缩请求数据
                            var body = DataHelper.Compress(byteData);
                            httpModel.Content = body;
                        }
                    }

                    natSession?.Send(PackHelper.CreatePack(new JsonData()
                    {
                        Type = (int)JsonType.HTTP,
                        Action = (int)HttpAction.Request,
                        Data = httpModel.ToJson()
                    }));
                }
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"HttpServer OnHandleContext穿透处理异常，{ex}");
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
                            var context = ContextDict.FirstOrDefault(c => c.Key == httpModel.SessionId).Value;
                            if (context == null)
                            {
                                HandleLog.WriteLine($"未找到上下文context，SessionId={httpModel.SessionId}");
                                return;
                            }
                            using var stream = context.Response.OutputStream;
                            if (httpModel.Content?.Length > 0)
                            {
                                //解压
                                var byteData = DataHelper.Decompress(httpModel.Content);
                                var res = byteData.ToUTF8String();
                                foreach (var item in httpModel.Headers)
                                {
                                    context.Response.AddHeader(item.Key, item.Value);
                                }
                                context.Response.StatusCode = httpModel.StatusCode;
                                context.Response.ContentType = httpModel.ContentType;
                                context.Response.ContentLength64 = byteData.Length;
                                //把处理信息返回到客户端
                                stream.WriteAsync(byteData, 0, byteData.Length).ContinueWith((t) =>
                                {
                                    var timeSpan = (DateTime.Now - httpModel.RequestTime);
                                    var totalSize = byteData.Length * 1.00 / 1024;
                                    var speed = Math.Round(totalSize / timeSpan.TotalSeconds, 1);
                                    HandleLog.WriteLine($"{session.Client.user_name} {session.Client.name} {context.Request.HttpMethod} {context.Request.Url.AbsoluteUri} {httpModel.StatusCode} {httpModel.StatusMessage} {Math.Round(totalSize, 1)}KB {timeSpan.TotalMilliseconds}ms {speed}KB/s");
                                    ContextDict.Remove(httpModel.SessionId);
                                });
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"HttpServer ProcessData穿透处理异常，{ex}");
            }
        }

        public void Stop()
        {
            HttpListener?.Stop();
        }
    }
}
