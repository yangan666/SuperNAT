using log4net;
using log4net.Config;
using log4net.Repository;
using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperNAT.Client
{
    public class ClientHandler
    {
        public static string CertFile = "iot3rd.p12";
        public static string CertPassword = "IoM@1234";
        public static NatClient NatClient { get; set; }
        public static List<TcpClientProxy> TcpClientProxyList { get; set; } = new List<TcpClientProxy>();
        public static string Secret { get; set; } = AppConfig.GetSetting("Secret");
        public static string ServerUrl { get; set; } = AppConfig.GetSetting("ServerUrl");
        public static int ServerPort { get; set; } = Convert.ToInt32(AppConfig.GetSetting("ServerPort"));
        public static int NatPort { get; set; } = Convert.ToInt32(AppConfig.GetSetting("NatPort"));
        public static ILoggerRepository Repository { get; set; }
        public static Thread reConnectThread, heartThread;
        public static bool IsReConnect = true;


        public void Start()
        {
            HandleLog.WriteLog += (log, isPrint) =>
            {
                if (isPrint)
                {
                    Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss,ffff} {log}");
                }
                Log4netUtil.Info(log);
            };
            Task.Run(() =>
            {
                try
                {
                    Repository = LogManager.CreateRepository("NETCoreRepository");
                    XmlConfigurator.Configure(Repository, new FileInfo("log4net.config"));
                    Log4netUtil.LogRepository = Repository;//类库中定义的静态变量
                    //连接服务器
                    ConnectNatServer();
                    //开启重连线程
                    reConnectThread = new Thread(ReConnect) { IsBackground = true };
                    reConnectThread.Start();
                    //开启心跳线程
                    heartThread = new Thread(SendHeart) { IsBackground = true };
                    heartThread.Start();
                }
                catch (Exception ex)
                {
                    HandleLog.WriteLine($"Start Error:{ex}");
                }
            });
        }

        public void Stop()
        {
            reConnectThread?.Abort();
            reConnectThread = null;

            heartThread?.Abort();
            heartThread = null;

            NatClient?.Close();
            NatClient = null;
        }

        static bool isLock = false;
        static async void ConnectNatServer()
        {
            try
            {
                if (!IsReConnect)
                {
                    return;
                }
                isLock = true;
                HandleLog.WriteLine($"正在连接服务器...");
                //解析主机名
                IPHostEntry ipInfo = Dns.GetHostEntry(ServerUrl);
                var serverIp = ipInfo.AddressList.Any() ? ipInfo.AddressList[0].ToString() : throw new Exception($"域名【{ServerUrl}】无法解析");
                NatClient = new NatClient(new ClientOptions()
                {
                    Ip = serverIp,
                    Port = NatPort,
                    NoDelay = true,
                    ProtocolType = ProtocolType.Tcp,
                    Security = SslProtocols.Tls12,
                    SslClientAuthenticationOptions = new SslClientAuthenticationOptions
                    {
                        EnabledSslProtocols = SslProtocols.Tls12,
                        TargetHost = serverIp,
                        ClientCertificates = new X509CertificateCollection() { new X509Certificate(CertFile, CertPassword) }
                    }
                });
                NatClient.Initialize(new NatReceiveFilter());
                NatClient.OnConnected += OnClientConnected;
                NatClient.OnReceived += OnPackageReceived;
                NatClient.OnClosed += OnClientClosed;

                await NatClient.ConnectAsync();
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"连接服务器失败：{ex}");
            }

            isLock = false;
        }

        static void ReConnect()
        {
            while (IsReConnect)
            {
                try
                {
                    Thread.Sleep(3000);
                    if (!isLock && (NatClient == null || !NatClient.IsConnected))
                    {
                        //重连
                        HandleLog.WriteLine("尝试重新连接服务器...");
                        ConnectNatServer();
                    }
                }
                catch (Exception ex)
                {
                    HandleLog.WriteLine($"尝试重新连接服务器失败：{ex}");
                }
            }
        }

        static void SendHeart()
        {
            while (IsReConnect)
            {
                Thread.Sleep(50000);
                if (NatClient.IsConnected)
                {
                    //发送心跳包给服务端
                    var pack = PackHelper.CreatePack(new JsonData()
                    {
                        Type = (int)JsonType.NAT,
                        Action = (int)NatAction.Heart,
                        Data = Secret
                    });
                    NatClient?.Send(pack);
                }
            }
        }

        static void OnClientConnected(Socket socket)
        {
            //发送注册包给服务端
            var pack = PackHelper.CreatePack(new JsonData()
            {
                Type = (int)JsonType.NAT,
                Action = (int)NatAction.Connect,
                Data = Secret
            });
            NatClient?.Send(pack);
        }

        static void OnPackageReceived(Socket socket, NatPackageInfo packageInfo)
        {
            Task.Run(() =>
            {
                switch (packageInfo.Body.Type)
                {
                    case (byte)JsonType.NAT:
                        NatClient.ProcessData(socket, packageInfo);
                        break;
                    case (byte)JsonType.HTTP:
                        HttpClientProxy.ProcessData(NatClient, packageInfo);
                        break;
                    case (byte)JsonType.TCP:
                        {
                            int waitTimes = 50;
                            var tcpModel = packageInfo.Body.Data.FromJson<TcpModel>();
                            TcpClientProxy clientProxy = null;
                        mark:
                            clientProxy = TcpClientProxyList.Find(c => c.RemoteSession.SessionId == tcpModel.SessionId);
                            if (packageInfo.Body.Action == (int)TcpAction.TransferData)
                            {
                                if ((clientProxy == null || !clientProxy.IsConnected) && waitTimes >= 0)
                                {
                                    HandleLog.WriteLine($"----> {tcpModel.SessionId} 未连接  IsConnected={clientProxy?.IsConnected.ToString() ?? "NULL"} ProxyCount={TcpClientProxyList.Count}");
                                    Thread.Sleep(100);
                                    waitTimes--;
                                    goto mark;
                                }
                            }
                            if (clientProxy == null)
                            {
                                var arr = tcpModel.Local.Split(":");
                                var ip = arr[0];
                                int.TryParse(arr[1], out int port);
                                clientProxy = new TcpClientProxy(new ClientOptions()
                                {
                                    Ip = ip,
                                    Port = port,
                                    NoDelay = true,
                                    ProtocolType = ProtocolType.Tcp
                                })
                                {
                                    NatClient = NatClient
                                };
                            }
                            clientProxy.ProcessData(NatClient, packageInfo);
                            break;
                        }
                }
            });
        }

        static void OnClientClosed(Socket socket)
        {
            HandleLog.WriteLine($"NatClient{NatClient.Local}已关闭");
        }
    }
}
