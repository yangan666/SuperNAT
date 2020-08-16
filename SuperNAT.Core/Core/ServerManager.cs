using SuperNAT.AsyncSocket;
using SuperNAT.Bll;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Core
{
    public class ServerManager
    {
        public static NatServer NATServer { get; set; }
        public static List<Map> MapList { get; set; } = new List<Map>();
        public static List<HttpServer> HttpServerList { get; set; } = new List<HttpServer>();
        public static List<TcpServer> TcpServerList { get; set; } = new List<TcpServer>();
        public static void Start()
        {
            //开启内网TCP服务
            StartNATServer(GlobalConfig.NatPort);
            //拉取Map列表到缓存
            GetMapList();
            //启动所有服务
            StartAllServer();
        }
        public static void GetMapList()
        {
            MapBll mapBll = new MapBll();
            MapList = mapBll.GetList(new Map()).Data ?? new List<Map>();
        }
        public static void ChangeMap(int type, Map map)
        {
            try
            {
                map.ChangeType = type;
                ChangeMapList(map);

                var natClient = NATServer.GetSingleSession(c => c.Client?.id == map.client_id);
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
                HandleLog.Log($"映射更新异常：{ex}，参数为：{map.ToJson()}");
            }
        }
        private static void ChangeMap(Map map, NatSession session)
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
                HandleLog.Log($"Session映射{Enum.GetName(typeof(ChangeMapType), map.ChangeType)}成功：{JsonHelper.Instance.Serialize(map)}", false);
                HandleLog.Log($"【{map.name}】Session映射{Enum.GetName(typeof(ChangeMapType), map.ChangeType)}成功：{map.local_endpoint} --> {map.remote_endpoint}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private static void ChangeMapList(Map map)
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
                HandleLog.Log($"服务映射缓存{Enum.GetName(typeof(ChangeMapType), map.ChangeType)}成功：{map.ToJson()}", false);
                HandleLog.Log($"【{map.name}】服务映射缓存{Enum.GetName(typeof(ChangeMapType), map.ChangeType)}成功：{map.local_endpoint} --> {map.remote_endpoint}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                HandleLog.Log($"启动服务失败：{ex}");
            }
        }

        #region NAT传输服务
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
                    NATServer.StartAysnc();
                    HandleLog.Log($"NAT服务启动成功，监听端口：{port}");
                }
                catch (Exception ex)
                {
                    HandleLog.Log($"NAT服务启动失败，端口：{port}，{ex}");
                }
            });
        }

        private static void Connected(NatSession session)
        {
            HandleLog.Log($"内网客户端【{session.RemoteEndPoint}】已连接");
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
                                HandleLog.Log($"NAT收到数据：{requestInfo.Raw.ToHexWithSpace()},正文内容: {requestInfo.Body.ToJson()}", false);
                                NATServer.ProcessData(session, requestInfo);
                                break;
                            }
                        case (byte)JsonType.HTTP:
                            {
                                var httpModel = requestInfo.Body.Data.FromJson<HttpModel>();
                                var server = HttpServerList.Find(c => c.ServerId == httpModel.ServerId);
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
                    HandleLog.Log($"穿透传输连接【{session.RemoteEndPoint},{session.Client?.name}】响应请求异常：{ex}");
                }
            });
        }

        private static void Closed(NatSession session)
        {
            HandleLog.Log($"内网客户端【{session.RemoteEndPoint}】已下线");
            if (session.Client != null)
            {
                Task.Run(() =>
                {
                    //更新在线状态
                    var bll = new ClientBll();
                    var updateRst = bll.UpdateOnlineStatus(new Client() { secret = session.Client.secret, is_online = false });
                    HandleLog.Log($"更新主机【{session.Client.name}】离线状态结果：{updateRst.Message}");
                });
            }
        }
        #endregion

        #region Web服务
        private static void StartHttpsServer(ServerConfig serverConfig)
        {
            try
            {
                foreach (var port in serverConfig.port_list)
                {
                    Task.Run(async () =>
                    {
                        var server = new HttpServer(new ServerOption()
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

                        var res = await server.StartAysnc();
                        if (res)
                        {
                            HttpServerList.Add(server);
                        }
                        HandleLog.Log($"{serverConfig.protocol}服务启动{"成功".If(res, "失败")}，监听端口：{port}");
                    });
                }
            }
            catch (Exception ex)
            {
                HandleLog.Log($"{serverConfig.protocol}服务初始化失败，端口：{serverConfig.port}，{ex}");
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
                    Task.Run(async () =>
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

                        var res = await server.StartAysnc();
                        if (res)
                        {
                            TcpServerList.Add(server);
                        }
                        HandleLog.Log($"{serverConfig.protocol}服务启动{"成功".If(res, "失败")}，监听端口：{port}");
                    });
                }
            }
            catch (Exception ex)
            {
                HandleLog.Log($"{serverConfig.protocol}服务初始化失败，端口：{serverConfig.port}，{ex}");
            }
        }
        #endregion


        public static void Stop()
        {
            NATServer?.Stop();
            HttpServerList.ForEach(c => c.Stop());
            TcpServerList.ForEach(c => c.Stop());
            MapList.Clear();
        }
    }
}
