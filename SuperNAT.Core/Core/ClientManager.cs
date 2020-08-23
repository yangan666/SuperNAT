using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperNAT.Core
{
    public class ClientManager
    {
        public static string CertFile = "iot3rd.p12";
        public static string CertPassword = "IoM@1234";
        public static NatClient NatClient { get; set; }
        public static List<TcpClientProxy> TcpClientProxyList { get; set; } = new List<TcpClientProxy>();
        public static string Secret { get; set; }
        public static string ServerUrl { get; set; }
        public static int ServerPort { get; set; }
        public static int NatPort { get; set; }
        public static Thread reConnectThread, heartThread;
        public static bool IsReConnect = true;
        static object lockLog = new object();//日志锁
        public static void Start()
        {
            Task.Run(() =>
            {
                try
                {
                    //初始化配置
                    var builder = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", false);
                    var configuration = builder.Build();

                    Secret = configuration.GetValue<string>("Secret");
                    ServerUrl = configuration.GetValue<string>("ServerUrl");
                    ServerPort = configuration.GetValue<int>("ServerPort");
                    NatPort = configuration.GetValue<int>("NatPort");

                    var repository = LogManager.CreateRepository("NETCoreRepository");
                    XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
                    Log4netUtil.LogRepository = repository;//类库中定义的静态变量
                    LogHelper.WriteLog += (level, log, isPrint) =>
                    {
                        lock (lockLog)
                        {
                            switch (level)
                            {
                                case LogLevel.Debug:
                                    Log4netUtil.Debug(log);
                                    break;
                                case LogLevel.Information:
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Log4netUtil.Info(log);
                                    break;
                                case LogLevel.Warning:
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Log4netUtil.Warn(log);
                                    break;
                                case LogLevel.Error:
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Log4netUtil.Error(log);
                                    break;
                                case LogLevel.Critical:
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    Log4netUtil.Fatal(log);
                                    break;
                                default:
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Log4netUtil.Info(log);
                                    break;
                            }
                            if (isPrint)
                            {
                                Console.Write(LogHelper.GetString(level));
                                Console.ResetColor();
                                Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss,ffff} {log}");
                            }
                        }
                    };

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
                    LogHelper.Info($"Start Error:{ex}");
                }
            });
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
                LogHelper.Info($"正在连接服务器...");
                //解析主机名
                var serverIp = ServerUrl;
                var ipArr = ServerUrl.Split('.');
                //先判断是不是IP，不是IP就尝试解析域名
                if (ipArr.Where(c => int.TryParse(c, out int i) && i > 0 && i <= 255).Count() != 4)
                {
                    IPHostEntry ipInfo = Dns.GetHostEntry(ServerUrl);
                    serverIp = ipInfo.AddressList.Any() ? ipInfo.AddressList[0].ToString() : throw new Exception($"域名【{ServerUrl}】无法解析");
                }
                NatClient = new NatClient(new ClientOption()
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
                NatClient.OnConnected += OnClientConnected;
                NatClient.OnReceived += OnPackageReceived;
                NatClient.OnClosed += OnClientClosed;

                await NatClient.ConnectAsync();
            }
            catch (Exception ex)
            {
                LogHelper.Error($"连接服务器失败：{ex}");
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
                        LogHelper.Error("尝试重新连接服务器...");
                        ConnectNatServer();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"尝试重新连接服务器失败：{ex}");
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

        static void OnClientConnected(object sender)
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

        static void OnPackageReceived(object sender, NatRequestInfo natRequestInfo)
        {
            Task.Run(() =>
            {
                switch (natRequestInfo.Body.Type)
                {
                    case (byte)JsonType.NAT:
                        NatClient.ProcessData(natRequestInfo);
                        break;
                    case (byte)JsonType.HTTP:
                        HttpClientProxy.ProcessData(NatClient, natRequestInfo);
                        break;
                    case (byte)JsonType.TCP:
                        {
                            int waitTimes = 50;
                            var tcpModel = natRequestInfo.Body.Data.FromJson<TcpModel>();
                            TcpClientProxy clientProxy = null;
                            mark:
                            clientProxy = TcpClientProxyList.Find(c => c.RemoteSession.SessionId == tcpModel.SessionId);
                            if (natRequestInfo.Body.Action == (int)TcpAction.TransferData)
                            {
                                if ((clientProxy == null || !clientProxy.IsConnected) && waitTimes >= 0)
                                {
                                    LogHelper.Warning($"----> {tcpModel.SessionId} 未连接  IsConnected={clientProxy?.IsConnected.ToString() ?? "NULL"} ProxyCount={TcpClientProxyList.Count}");
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
                                clientProxy = new TcpClientProxy(new ClientOption()
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
                            clientProxy.ProcessData(NatClient, natRequestInfo);
                            break;
                        }
                }
            });
        }

        static void OnClientClosed(object sender)
        {
            LogHelper.Error($"NatClient{NatClient.LocalEndPoint}已关闭");
        }

        public static void Stop()
        {
            reConnectThread?.Abort();
            reConnectThread = null;

            heartThread?.Abort();
            heartThread = null;

            NatClient?.Close();
            NatClient = null;
        }
    }
}
