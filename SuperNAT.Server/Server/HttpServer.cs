using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

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
            Url = $"http://localhost:{port}/";
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
                    stream.Write(returnByteArr, 0, returnByteArr.Length);
                }
                else
                {
                    var sessionId = Guid.NewGuid().ToString();
                    ContextDict.Add(sessionId, context);

                    var httpModel = new HttpModel()
                    {
                        ServerId = ServerId,
                        Host = context.Request.Url.Authority,
                        SessionId = sessionId,
                        Method = context.Request.HttpMethod,
                        Route = context.Request.RawUrl,
                        Headers = context.Request.Headers
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
                        var req = byteData.ToASCII();

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
                                return;
                            var byteData = DataHelper.Decompress(httpModel.Content);
                            context.Response.OutputStream.WriteAsync(byteData, 0, byteData.Length);
                            context.Response.OutputStream.FlushAsync();
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
