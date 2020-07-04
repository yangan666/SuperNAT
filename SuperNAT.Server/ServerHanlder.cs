using SuperNAT.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using SuperNAT.Bll;
using SuperNAT.Model;
using System.Diagnostics;
using System.Security.Authentication;
using System.Net.Security;
using SuperNAT.AsyncSocket;
using System.Net.Sockets;

namespace SuperNAT.Server
{
    public class ServerHanlder
    {
        public static NatServer NATServer { get; set; }
        public static List<Map> MapList { get; set; } = new List<Map>();
        public static List<HttpServer> HttpServerList { get; set; } = new List<HttpServer>();
        public static List<HttpsServer> HttpsServerList { get; set; } = new List<HttpsServer>();
        public static List<TcpServer> TcpServerList { get; set; } = new List<TcpServer>();

        public void Start(string[] args)
        {
            Startup.Init();

            //开启内网TCP服务
            StartNATServer(GlobalConfig.NatPort);
            //拉取Map列表到缓存
            GetMapList();
            //启动所有服务
            StartAllServer();
            //接口服务
            Task.Run(() =>
            {
                CreateHostBuilder(args).Build().Run();
            });
        }

        public void Stop()
        {
            NATServer?.Stop();
            HttpServerList?.ForEach(c => c.Stop());
            TcpServerList?.ForEach(c => c.Stop());
        }

        public void GetMapList()
        {
            MapBll mapBll = new MapBll();
            MapList = mapBll.GetList(new Map()).Data ?? new List<Map>();
        }

        public static void StartAllServer()
        {
            try
            {
                var bll = new ServerConfigBll();
                var serverList = bll.GetList("").Data ?? new List<ServerConfig>();
                if (serverList.Any())
                {
                    foreach (var item in serverList)
                    {
                        switch (item.protocol)
                        {
                            case "http":
                            //StartHttpServer(item);
                            //break;
                            case "https":
                                StartHttpsServer(item);
                                break;
                            case "tcp":
                            case "udp":
                                StartTcpServer(item);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"启动服务失败：{ex}");
            }
        }

        public static void ChangeMap(int type, Map map)
        {
            try
            {
                map.ChangeType = type;
                ChangeMapList(map);

                var natClient = NATServer.GetSingle(c => c.Client?.id == map.client_id);
                if (natClient == null)
                    return;
                ChangeMap(map, natClient);
                var pack = PackHelper.CreatePack(new JsonData()
                {
                    Type = (int)JsonType.NAT,
                    Action = (int)NatAction.MapChange,
                    Data = map.ToJson()
                });
                natClient.Send(pack);
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"映射更新异常：{ex}，参数为：{JsonHelper.Instance.Serialize(map)}");
            }
        }

        static void ChangeMap(Map map, NatSession session)
        {
            try
            {
                switch (map.ChangeType)
                {
                    case (int)ChangeMapType.新增:
                        session.MapList.Add(map);
                        break;
                    case (int)ChangeMapType.修改:
                        session.MapList.RemoveAll(c => c.id == map.id);
                        session.MapList.Add(map);
                        break;
                    case (int)ChangeMapType.删除:
                        session.MapList.RemoveAll(c => c.id == map.id);
                        break;
                }
                HandleLog.WriteLine($"Session映射{Enum.GetName(typeof(ChangeMapType), map.ChangeType)}成功：{JsonHelper.Instance.Serialize(map)}", false);
                HandleLog.WriteLine($"【{map.name}】Session映射{Enum.GetName(typeof(ChangeMapType), map.ChangeType)}成功：{map.local_endpoint} --> {map.remote_endpoint}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static void ChangeMapList(Map map)
        {
            try
            {
                switch (map.ChangeType)
                {
                    case (int)ChangeMapType.新增:
                        MapList.Add(map);
                        break;
                    case (int)ChangeMapType.修改:
                        MapList.RemoveAll(c => c.id == map.id);
                        MapList.Add(map);
                        break;
                    case (int)ChangeMapType.删除:
                        MapList.RemoveAll(c => c.id == map.id);
                        break;
                }
                HandleLog.WriteLine($"服务映射缓存{Enum.GetName(typeof(ChangeMapType), map.ChangeType)}成功：{JsonHelper.Instance.Serialize(map)}", false);
                HandleLog.WriteLine($"【{map.name}】服务映射缓存{Enum.GetName(typeof(ChangeMapType), map.ChangeType)}成功：{map.local_endpoint} --> {map.remote_endpoint}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region 内网TCP服务
        private static void StartNATServer(int port)
        {
            Task.Run(() =>
            {
                try
                {
                    NATServer = new NatServer(new ServerOption()
                    {
                        Ip = "Any",
                        Port = port,
                        ProtocolType = ProtocolType.Tcp,
                        BackLog = 100,
                        NoDelay = true,
                        Security = SslProtocols.Tls12,
                        SslServerAuthenticationOptions = new SslServerAuthenticationOptions
                        {
                            EnabledSslProtocols = SslProtocols.Tls12,
                            ClientCertificateRequired = false,
                            ServerCertificate = new X509Certificate2(GlobalConfig.CertFile, GlobalConfig.CertPassword)
                        }
                    });
                    NATServer.OnConnected += Connected;
                    NATServer.OnReceived += Received;
                    NATServer.OnClosed += Closed;
                    NATServer.Start();
                    HandleLog.WriteLine($"NAT服务启动成功，监听端口：{port}");
                }
                catch (Exception ex)
                {
                    HandleLog.WriteLine($"NAT服务启动失败，端口：{port}，{ex}");
                }
            });
        }

        private static void Connected(NatSession session)
        {
            HandleLog.WriteLine($"内网客户端【{session.Remote}】已连接");
        }

        private static void Received(NatSession session, NatRequestInfo requestInfo)
        {
            Task.Run(() =>
            {
                try
                {
                    switch (requestInfo.Body.Type)
                    {
                        case (byte)JsonType.NAT:
                            {
                                HandleLog.WriteLine($"NAT收到数据：{requestInfo.Raw.ToHexWithSpace()},正文内容: {requestInfo.Body.ToJson()}");
                                NATServer.ProcessData(session, requestInfo);
                                break;
                            }
                        case (byte)JsonType.HTTP:
                            {
                                var httpModel = requestInfo.Body.Data.FromJson<HttpModel>();
                                var server = HttpsServerList.Find(c => c.ServerId == httpModel.ServerId);
                                server?.ProcessData(session, requestInfo, httpModel);
                                break;
                            }
                        case (byte)JsonType.TCP:
                            {
                                var tcpModel = requestInfo.Body.Data.FromJson<TcpModel>();
                                var server = TcpServerList.Find(c => c.ServerId == tcpModel.ServerId);
                                server?.ProcessData(session, requestInfo, tcpModel);
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    HandleLog.WriteLine($"穿透传输连接【{session.Remote},{session.Client?.name}】响应请求异常：{ex}");
                }
            });
        }

        private static void Closed(NatSession session)
        {
            HandleLog.WriteLine($"内网客户端【{session.Remote}】已下线");
            if (session.Client != null)
            {
                Task.Run(() =>
                {
                    //更新在线状态
                    var bll = new ClientBll();
                    var updateRst = bll.UpdateOnlineStatus(new Client() { secret = session.Client.secret, is_online = false });
                    HandleLog.WriteLine($"更新主机【{session.Client.name}】离线状态结果：{updateRst.Message}");
                });
            }
        }
        #endregion

        #region Web服务
        private static void StartHttpServer(ServerConfig serverConfig)
        {
            try
            {
                foreach (var port in serverConfig.port_list)
                {
                    var server = new HttpServer(port) { NATServer = NATServer };
                    server.Start();
                    HttpServerList.Add(server);
                }

                HandleLog.WriteLine($"{serverConfig.protocol}服务启动成功，监听端口：{serverConfig.port}");
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"{serverConfig.protocol}服务初始化失败，端口：{serverConfig.port}，{ex}");
            }
        }

        private static void StartHttpsServer(ServerConfig serverConfig)
        {
            try
            {
                foreach (var port in serverConfig.port_list)
                {
                    Task.Run(() =>
                    {
                        var server = new HttpsServer(new ServerOption()
                        {
                            Ip = "Any",
                            Port = port,
                            ProtocolType = ProtocolType.Tcp,
                            BackLog = 100,
                            NoDelay = true,
                            Security = serverConfig.is_ssl ? SslProtocols.Tls12 : SslProtocols.None,
                            SslServerAuthenticationOptions = serverConfig.is_ssl ? new SslServerAuthenticationOptions
                            {
                                EnabledSslProtocols = SslProtocols.Tls12,
                                ServerCertificate = new X509Certificate2(string.IsNullOrEmpty(serverConfig.certfile) ? GlobalConfig.CertFile : serverConfig.certfile, string.IsNullOrEmpty(serverConfig.certpwd) ? GlobalConfig.CertPassword : serverConfig.certpwd)
                            } : null
                        });

                        var res = server.Start();
                        if (res)
                        {
                            HttpsServerList.Add(server);
                        }
                        HandleLog.WriteLine($"{serverConfig.protocol}服务启动{"成功".If(res, "失败")}，监听端口：{port}");
                    });
                }
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"{serverConfig.protocol}服务初始化失败，端口：{serverConfig.port}，{ex}");
            }
        }
        #endregion

        #region Tcp服务
        private static void StartTcpServer(ServerConfig serverConfig)
        {
            try
            {
                foreach (var port in serverConfig.port_list)
                {
                    Task.Run(() =>
                    {
                        var server = new TcpServer(new ServerOption()
                        {
                            Ip = "Any",
                            Port = port,
                            ProtocolType = ProtocolType.Tcp,
                            BackLog = 100,
                            NoDelay = true,
                            Security = serverConfig.is_ssl ? SslProtocols.Tls12 : SslProtocols.None,
                            SslServerAuthenticationOptions = serverConfig.is_ssl ? new SslServerAuthenticationOptions
                            {
                                EnabledSslProtocols = SslProtocols.Tls12,
                                ServerCertificate = new X509Certificate2(string.IsNullOrEmpty(serverConfig.certfile) ? GlobalConfig.CertFile : serverConfig.certfile, string.IsNullOrEmpty(serverConfig.certpwd) ? GlobalConfig.CertPassword : serverConfig.certpwd)
                            } : null
                        });

                        var res = server.Start();
                        if (res)
                        {
                            TcpServerList.Add(server);
                        }
                        HandleLog.WriteLine($"{serverConfig.protocol}服务启动{"成功".If(res, "失败")}，监听端口：{port}");
                    });
                }
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"{serverConfig.protocol}服务初始化失败，端口：{serverConfig.port}，{ex}");
            }
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
                            //options.Listen(IPAddress.Any, GlobalConfig.ServerPort, listenOptions =>
                            //{
                            //    listenOptions.UseHttps(GlobalConfig.CertFile, GlobalConfig.CertPassword);
                            //});
                        })
                        .UseContentRoot(contentRoot)  // set content root
                        .UseWebRoot(webRoot);         // set web root
            });
        #endregion
    }
}
