//using SuperNAT.Common;
//using SuperNAT.Model;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Security;
//using System.Security.Cryptography.X509Certificates;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace SuperNAT.Server
//{
//    public class HttpAppServer : AppServer<WebAppSession, HttpRequestInfo>
//    {
//        public HttpAppServer()
//            : base(new DefaultReceiveFilterFactory<HttpReceiveFilter, HttpRequestInfo>())
//        {

//        }

//        public Common.Models.ServerConfig ServerConfig { get; set; }
//    }

//    public class HttpServer : IServer
//    {
//        public static string CertFile = "iot3rd.p12";
//        public static string CertPassword = "IoM@1234";
//        private HttpAppServer server;
//        public List<WebAppSession> sessions => server?.GetAllSessions().ToList() ?? new List<WebAppSession>();
//        public HttpServer(HttpAppServer httpAppServer)
//        {
//            server = httpAppServer;
//        }
//        public void Start()
//        {
//            var serverConfig = server.ServerConfig;
//            bool setup = server.Setup(new RootConfig()
//            {
//                DisablePerformanceDataCollector = true
//            }, new CSuperSocket.SocketBase.Config.ServerConfig()
//            {
//                Listeners = from s in serverConfig.port_list
//                            select new ListenerConfig
//                            {
//                                Ip = "Any",
//                                Port = s
//                            },//批量监听
//                TextEncoding = "ASCII",
//                MaxRequestLength = 102400,
//                MaxConnectionNumber = 1000,
//                ReceiveBufferSize = 102400,
//                SendBufferSize = 102400,
//                LogBasicSessionActivity = true,
//                LogAllSocketException = true,
//                SyncSend = false,
//                Security = serverConfig.ssl_type == null ? null : Enum.GetName(typeof(ssl_type), serverConfig.ssl_type),
//                Certificate = serverConfig.ssl_type == null ? null : new CertificateConfig()
//                {
//                    FilePath = string.IsNullOrEmpty(serverConfig.certfile) ? CertFile : serverConfig.certfile,
//                    Password = string.IsNullOrEmpty(serverConfig.certpwd) ? CertPassword : serverConfig.certpwd,
//                    ClientCertificateRequired = false
//                },
//                DisableSessionSnapshot = true,
//                SessionSnapshotInterval = 1
//            });
//            if (setup)
//            {
//                var start = server.Start();
//                if (start)
//                {
//                    server.NewSessionConnected += WebServer_NewSessionConnected;
//                    server.NewRequestReceived += WebServer_NewRequestReceived;
//                    server.SessionClosed += WebServer_SessionClosed;
//                    HandleLog.WriteLine($"{serverConfig.protocol}服务启动成功，监听端口：{serverConfig.port}");
//                }
//                else
//                {
//                    HandleLog.WriteLine($"{serverConfig.protocol}服务启动失败，端口：{serverConfig.port}");
//                }
//            }
//            else
//            {
//                HandleLog.WriteLine($"{serverConfig.protocol}服务初始化失败，端口：{serverConfig.port}");
//            }
//        }

//        public void Stop()
//        {
//            server?.Stop();
//        }

//        public void Response(PackJson packJson)
//        {
//            //02 01 数据长度(4) 正文数据(n)   ---http响应包
//            int count = 0;
//        mark:
//            var webSession = server.GetSessions(c => c.UserId.ToLower() == packJson.UserId.ToLower()).FirstOrDefault();
//            if (webSession == null)
//            {
//                count++;
//                Thread.Sleep(500);
//                if (count < 5)
//                {
//                    goto mark;
//                }
//                HandleLog.WriteLine($"webSession【{packJson.UserId}】不存在");
//                return;
//            }
//            //先讲16进制字符串转为byte数组  再gzip解压
//            var response = DataHelper.Decompress(packJson.Content);
//            var res = webSession.TrySend(response, 0, response.Length);
//            HandleLog.WriteLine($"{packJson.ResponseInfo} {Math.Ceiling((DateTime.Now - webSession.RequestTime).Value.TotalMilliseconds)}ms");
//        }

//        private static void WebServer_NewSessionConnected(WebAppSession session)
//        {
//            //HandleLog.WriteLine($"客户端【{session.SessionID}】已连接");
//        }

//        private static void WebServer_NewRequestReceived(WebAppSession session, HttpRequestInfo requestInfo)
//        {
//            Task.Run(() =>
//            {
//                try
//                {
//                    if (session.RequestTime == null)
//                    {
//                        session.RequestTime = DateTime.Now;
//                    }
//                    //转发请求
//                    var host = requestInfo.HeaderDict["Host"];
//                    var natSession = ServerHanlder.NATServer.GetSessions(c => c.MapList?.Any(m => m.remote_endpoint == host) ?? false).FirstOrDefault();
//                    if (natSession == null)
//                    {
//                        session?.Close();
//                        HandleLog.WriteLine($"请求：{host}失败，Nat客户端连接不在线！");
//                        return;
//                    }
//                    var pack = new PackJson()
//                    {
//                        Host = host,
//                        UserId = session.UserId,
//                        Method = requestInfo.Method,
//                        Route = requestInfo.Route,
//                        Headers = requestInfo.HeaderDict,
//                        Content = requestInfo.Content
//                    };
//                    var json = JsonHelper.Instance.Serialize(pack);
//                    var jsonBytes = Encoding.UTF8.GetBytes(json);
//                    //02 01 数据长度(4) 正文数据(n)   ---http响应包
//                    var sendBytes = new List<byte>() { 0x2, 0x1 };
//                    sendBytes.AddRange(BitConverter.GetBytes(jsonBytes.Length).Reverse());
//                    sendBytes.AddRange(jsonBytes);
//                    natSession.Send(sendBytes.ToArray(), 0, sendBytes.Count);
//                }
//                catch (Exception ex)
//                {
//                    HandleLog.WriteLine($"【{session.RemoteEndPoint}】请求参数：{Encoding.UTF8.GetString(requestInfo.Data)}，处理发生异常：{ex}");
//                }
//            });
//        }

//        private static void WebServer_SessionClosed(WebAppSession session, CSuperSocket.SocketBase.CloseReason value)
//        {
//            //HandleLog.WriteLine($"客户端【{session.SessionID}】已下线：{value}");
//        }
//    }
//}
