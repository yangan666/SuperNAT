using SuperNAT.Common;
using CSuperSocket.SocketBase;
using CSuperSocket.SocketBase.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSuperSocket.ProtoBase;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using SuperNAT.Common.Bll;
using SuperNAT.Common.Models;
using System.Diagnostics;

namespace SuperNAT.Server
{
    public class ServerHanlder
    {
        public static string CertFile = "iot3rd.p12";
        public static string CertPassword = "IoM@1234";
        public static HttpAppServer HttpServer { get; set; }
        public static NatAppServer NATServer { get; set; }

        public void Start(string[] args)
        {
            Dapper.SimpleCRUD.SetDialect(Dapper.SimpleCRUD.Dialect.MySQL);
            Startup.Init();
            //开启内网TCP服务
            Task.Run(StartNATServer);
            //开启外网Web服务
            Task.Run(StartWebServer);
            //接口服务
            Task.Run(() =>
            {
                CreateHostBuilder(args).Build().Run();
            });
            //管理后台修改映射通知客户端
            HandleEvent.MapAction += ChangeMap;
        }

        public void Stop()
        {
            HttpServer.Stop();
            NATServer.Stop();
        }

        public void ChangeMap(int type, Map map)
        {
            map.ChangeType = type;
            ChangeMap(map);
            //请求头 01 04 长度(4)
            var sendBytes = new List<byte>() { 0x1, 0x4 };
            var jsonBytes = Encoding.UTF8.GetBytes(JsonHelper.Instance.Serialize(map));
            sendBytes.AddRange(BitConverter.GetBytes(jsonBytes.Length).Reverse());
            sendBytes.AddRange(jsonBytes);
            var natClient = NATServer.GetSessions(c => c.Client.id == map.client_id).FirstOrDefault();
            natClient?.Send(sendBytes.ToArray());
        }

        static void ChangeMap(Map map)
        {
            var session = NATServer.GetSessions(c => c.Client.id == map.client_id).FirstOrDefault();
            if (session == null)
            {
                return;
            }
            if (session.MapList == null)
            {
                session.MapList = new List<Map>();
            }
            switch (map.ChangeType)
            {
                case (int)ChangeMapType.新增:
                    session.MapList.Add(map);
                    break;
                case (int)ChangeMapType.修改:
                    var item = session.MapList.Find(c => c.id == map.id);
                    if (item != null)
                    {
                        item = map;
                    }
                    else
                    {
                        session.MapList.Add(map);
                    }
                    break;
                case (int)ChangeMapType.删除:
                    session.MapList.RemoveAll(c => c.id == map.id);
                    break;
            }
            HandleLog.WriteLine($"映射{Enum.GetName(typeof(ChangeMapType), map.ChangeType)}成功：{JsonHelper.Instance.Serialize(map)}", false);
            HandleLog.WriteLine($"【{map.name}】映射{Enum.GetName(typeof(ChangeMapType), map.ChangeType)}成功：{map.local} --> {map.remote}");
        }

        #region 内网TCP服务
        private static void StartNATServer()
        {
            NATServer = new NatAppServer();
            var setup = NATServer.Setup(new RootConfig()
            {
                DisablePerformanceDataCollector = true
            }, new ServerConfig()
            {
                Ip = "Any",
                Port = GlobalConfig.NatPort,
                TextEncoding = "ASCII",
                MaxRequestLength = 102400,
                MaxConnectionNumber = 1000,
                ReceiveBufferSize = 102400,
                SendBufferSize = 102400,
                LogBasicSessionActivity = true,
                LogAllSocketException = true,
                SyncSend = true,
                Security = "tls12",
                Certificate = new CertificateConfig()
                {
                    FilePath = CertFile,
                    Password = CertPassword,
                    ClientCertificateRequired = false
                },
                DisableSessionSnapshot = true,
                SessionSnapshotInterval = 1
            });
            if (setup)
            {
                var start = NATServer.Start();
                if (start)
                {
                    NATServer.NewSessionConnected += NATServer_NewSessionConnected;
                    NATServer.NewRequestReceived += NATServer_NewRequestReceived;
                    NATServer.SessionClosed += NATServer_SessionClosed;
                    HandleLog.WriteLine($"NAT服务启动成功，监听端口：{NATServer.Config.Port}");
                }
            }
            else
            {
                HandleLog.WriteLine($"NAT服务启动失败，端口：{NATServer.Config.Port}");
            }
        }

        private static void NATServer_NewSessionConnected(NatAppSession session)
        {
            HandleLog.WriteLine($"内网客户端【{session.RemoteEndPoint}】已连接");
        }

        private static void NATServer_NewRequestReceived(NatAppSession session, NatRequestInfo requestInfo)
        {
            try
            {
                //HandleLog.WriteLine($"NAT服务收到数据：{requestInfo.Hex}");
                switch (requestInfo.FunCode)
                {
                    case 0x1:
                        {
                            //注册包
                            var secret = requestInfo.BodyRaw;
                            using var bll = new ClientBll();
                            var client = bll.GetOne(secret).Data;
                            if (client == null)
                            {
                                HandleLog.WriteLine($"连接【{session.RemoteEndPoint}】Token非法！！");
                                //通知客户端
                                
                                session.Send(sendBytes.ToArray());
                                return;
                            }
                            var sessionList = NATServer.GetSessions(c => c.Client?.secret == secret).ToList();
                            if (sessionList?.Count > 0)
                            {
                                sessionList.ForEach(c =>
                                {
                                    c?.Close();
                                    HandleLog.WriteLine($"【{session.Client.secret}】连接重复，强制关闭{c.RemoteEndPoint}");
                                });
                                return;
                            }
                            session.Client = client;

                            using var mapBll = new MapBll();
                            session.MapList = mapBll.GetMapList(secret).Data ?? new List<Map>();
                            //原样返回回复客户端注册成功
                            session.Send(requestInfo.Data);
                            Task.Run(() =>
                            {
                                //更新在线状态
                                using var bll = new ClientBll();
                                var updateRst = bll.UpdateOnlineStatus(new Client() { secret = session.Client.secret, is_online = true, last_heart_time = DateTime.Now });
                                HandleLog.WriteLine($"更新主机【{session.Client.name}】在线状态结果：{updateRst.Message}", false);
                            });
                        }
                        break;
                    case 0x2:
                        {
                            //心跳包
                            var secret = requestInfo.BodyRaw;
                            HandleLog.WriteLine($"收到连接{session.RemoteEndPoint}的心跳包，密钥为：{secret}，当前映射个数：{session.MapList.Count}", false);
                            Task.Run(() =>
                            {
                                //更新在线状态
                                using var bll = new ClientBll();
                                var updateRst = bll.UpdateOnlineStatus(new Client() { secret = session.Client.secret, is_online = true, last_heart_time = DateTime.Now });
                                HandleLog.WriteLine($"更新主机【{session.Client.name}】在线状态结果：{updateRst.Message}", false);
                            });
                        }
                        break;
                    case 0x3:
                        {
                            //响应请求
                            var packJson = JsonHelper.Instance.Deserialize<PackJson>(requestInfo.BodyRaw);
                            int count = 0;
                            mark:
                            var webSession = HttpServer.GetSessions(c => c.UserId.ToLower() == packJson.UserId.ToLower()).FirstOrDefault();
                            if (webSession == null)
                            {
                                count++;
                                Thread.Sleep(500);
                                if (count < 5)
                                {
                                    goto mark;
                                }
                                HandleLog.WriteLine($"webSession【{packJson.UserId}】不存在");
                                return;
                            }
                            //先讲16进制字符串转为byte数组  再gzip解压
                            var response = DataHelper.Decompress(packJson.Content);
                            var rawResponse = Encoding.UTF8.GetString(response);
                            var res = webSession.TrySend(response, 0, response.Length);
                            HandleLog.WriteLine($"{packJson.ResponseInfo} {Math.Ceiling((DateTime.Now - webSession.RequestTime).Value.TotalMilliseconds)}ms");
                            //webSession?.Close();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"webSession响应请求异常：{ex}");
            }
        }

        private static void NATServer_SessionClosed(NatAppSession session, CSuperSocket.SocketBase.CloseReason value)
        {
            HandleLog.WriteLine($"内网客户端【{session.RemoteEndPoint}】已下线");
            Task.Run(() =>
            {
                //更新在线状态
                using var bll = new ClientBll();
                var updateRst = bll.UpdateOnlineStatus(new Client() { secret = session.Client.secret, is_online = false });
                HandleLog.WriteLine($"更新主机【{session.Client.name}】离线状态结果：{updateRst.Message}");
            });
        }
        #endregion

        #region 网页Web服务
        private static void StartWebServer()
        {
            if (GlobalConfig.WebPortList.Any())
            {
                HttpServer = new HttpAppServer();
                bool setup = HttpServer.Setup(new RootConfig()
                {
                    DisablePerformanceDataCollector = true
                }, new ServerConfig()
                {
                    Listeners = from s in GlobalConfig.WebPortList
                                select new ListenerConfig
                                {
                                    Ip = "Any",
                                    Port = s
                                },//批量监听
                    TextEncoding = "ASCII",
                    MaxRequestLength = 102400,
                    MaxConnectionNumber = 1000,
                    ReceiveBufferSize = 102400,
                    SendBufferSize = 102400,
                    LogBasicSessionActivity = true,
                    LogAllSocketException = true,
                    SyncSend = false,
                    //Security = "tls12",
                    //Certificate = new CertificateConfig()
                    //{
                    //    FilePath = CertFile,
                    //    Password = CertPassword,
                    //    ClientCertificateRequired = false
                    //},
                    DisableSessionSnapshot = true,
                    SessionSnapshotInterval = 1
                });
                if (setup)
                {
                    var start = HttpServer.Start();
                    if (start)
                    {
                        HttpServer.NewSessionConnected += WebServer_NewSessionConnected;
                        HttpServer.NewRequestReceived += WebServer_NewRequestReceived;
                        HttpServer.SessionClosed += WebServer_SessionClosed;
                        HandleLog.WriteLine($"Web服务启动成功，监听端口：{GlobalConfig.WebPort}");
                    }
                    else
                    {
                        HandleLog.WriteLine($"Web服务启动失败，端口：{GlobalConfig.WebPort}");
                    }
                }
                else
                {
                    HandleLog.WriteLine($"Web服务初始化失败，端口：{GlobalConfig.WebPort}");
                }
            }
        }

        private static void WebServer_NewSessionConnected(WebAppSession session)
        {
            //HandleLog.WriteLine($"客户端【{session.SessionID}】已连接");
        }

        private static void WebServer_NewRequestReceived(WebAppSession session, HttpRequestInfo requestInfo)
        {
            Task.Run(() =>
            {
                try
                {
                    if (session.RequestTime == null)
                    {
                        session.RequestTime = DateTime.Now;
                    }
                    //转发请求
                    var host = requestInfo.HeaderDict["Host"];
                    var natSession = NATServer.GetSessions(c => c.MapList?.Any(m => m.remote == host) ?? false).FirstOrDefault();
                    if (natSession == null)
                    {
                        session?.Close();
                        HandleLog.WriteLine($"请求：{host}失败，Nat客户端连接不在线！");
                        return;
                    }
                    var pack = new PackJson()
                    {
                        Host = host,
                        UserId = session.UserId,
                        Method = requestInfo.Method,
                        Route = requestInfo.Route,
                        Headers = requestInfo.HeaderDict,
                        Content = requestInfo.Content
                    };
                    var json = JsonHelper.Instance.Serialize(pack);
                    var jsonBytes = Encoding.UTF8.GetBytes(json);
                    //请求头 01 03 长度(4)
                    var sendBytes = new List<byte>() { 0x1, 0x3 };
                    sendBytes.AddRange(BitConverter.GetBytes(jsonBytes.Length).Reverse());
                    sendBytes.AddRange(jsonBytes);
                    natSession.Send(sendBytes.ToArray(), 0, sendBytes.Count);
                }
                catch (Exception ex)
                {
                    HandleLog.WriteLine($"【{session.RemoteEndPoint}】请求参数：{Encoding.UTF8.GetString(requestInfo.Data)}，处理发生异常：{ex}");
                }
            });
        }

        private static void WebServer_SessionClosed(WebAppSession session, CSuperSocket.SocketBase.CloseReason value)
        {
            //HandleLog.WriteLine($"客户端【{session.SessionID}】已下线：{value}");
        }
        #endregion

        #region SuperNAT管理后台 8088端口
        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                var contentRoot = Directory.GetCurrentDirectory();
                var webRoot = Path.Combine(contentRoot, "wwwroot");
                webBuilder.UseStartup<Startup>()
                        .UseKestrel(options =>
                        {
                            options.Limits.MaxConcurrentConnections = 100;
                            options.Limits.MaxConcurrentUpgradedConnections = 100;
                            options.Limits.MaxRequestBodySize = 104857600;//100M
                            options.Limits.MinRequestBodyDataRate =
                                new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                            options.Limits.MinResponseDataRate =
                                new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                            options.Listen(IPAddress.Any, GlobalConfig.ServerPort);
                            //options.Listen(IPAddress.Loopback, 5001, listenOptions =>
                            //{
                            //    listenOptions.UseHttps("testCert.pfx", "testPassword");
                            //});
                        })
                        .UseContentRoot(contentRoot)  // set content root
                        .UseWebRoot(webRoot);         // set web root
            });
        #endregion
    }
}
