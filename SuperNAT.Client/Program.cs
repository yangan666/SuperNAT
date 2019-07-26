using SuperNAT.Common;
using SuperSocket.ClientEngine;
using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperNAT.Client
{
    class Program
    {
        public static EasyClient<NatPackageInfo> NatClient { get; set; }
        public static EasyClient<NatPackageInfo> HttpClient { get; set; }
        public static string NatAddress { get; set; }
        public static string RemoteHost { get; set; } = "127.0.0.1"; //AppConfig.GetSetting("RemoteHost");//139.155.104.69
        public static int RemoteWebPort { get; set; } = 10005;
        public static int RemoteNatPort { get; set; } = 10006;
        public static string Token { get; set; } = "3d951d8b2275425887e1e9d1e53c5fa5";
        public static string RegPack { get; set; } = $"{Token} {RemoteHost}:{RemoteWebPort}";
        public static byte[] RegPack2Bytes => Encoding.UTF8.GetBytes(RegPack);
        public static int LocalPort { get; set; }
        static void Main(string[] args)
        {
            HandleLog.WriteLog += (log) =>
            {
                Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss,ffff} {log}");
                Log4netUtil.Info(log);
            };
            mark:
            Console.Write("请输入本地映射端口：");
            var port = Console.ReadLine();
            var tryParse = int.TryParse(port, out int readPort);
            if (!tryParse)
            {
                Console.WriteLine("请输入正确的端口！");
                Console.WriteLine("按任意键继续...");
                Console.ReadKey();
                goto mark;
            }
            LocalPort = readPort;
            NatAddress = $"http://localhost:{readPort}";//http://localhost:8080/WinTar-I-EMS/
            //HandleLog.WriteLine("开始连接服务器...");
            ConnectNatServer();

            Thread reConnectThread = new Thread(ReConnect) { IsBackground = true };
            reConnectThread.Start();

            Thread heartThread = new Thread(SendHeart) { IsBackground = true };
            heartThread.Start();

            Console.ReadKey();
        }

        static void ConnectNatServer()
        {
            NatClient?.Close();
            NatClient = null;
            NatClient = new EasyClient<NatPackageInfo>
            {
                Security = new SecurityOption()
                {
                    EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                    AllowNameMismatchCertificate = true,
                    AllowCertificateChainErrors = true,
                    AllowUnstrustedCertificate = true
                }
            };
            NatClient.Initialize(new NatReceiveFilter());
            NatClient.Connected += OnClientConnected;
            NatClient.NewPackageReceived += OnPackageReceived;
            NatClient.Error += OnClientError;
            NatClient.Closed += OnClientClosed;
            //连接NAT转发服务
            NatClient.ConnectAsync(new IPEndPoint(IPAddress.Parse(RemoteHost), RemoteNatPort));
        }

        static void ReConnect()
        {
            while (true)
            {
                Thread.Sleep(5000);
                if (!NatClient.IsConnected)
                {
                    //重连
                    HandleLog.WriteLine("尝试重新连接服务器...");
                    ConnectNatServer();
                }
            }
        }

        static void SendHeart()
        {
            while (true)
            {
                if (NatClient.IsConnected)
                {
                    //发送心跳包
                    var packBytes = new List<byte>() { 0x1, 0x2 };
                    var lenBytes = BitConverter.GetBytes(RegPack2Bytes.Length).Reverse();
                    packBytes.AddRange(lenBytes);
                    packBytes.AddRange(RegPack2Bytes);
                    NatClient.Send(packBytes.ToArray());
                }
                Thread.Sleep(30000);
            }
        }

        static void OnClientConnected(object sender, EventArgs e)
        {
            //HandleLog.WriteLine($"【{NatClient.LocalEndPoint}】已连接到服务器【{NatClient.Socket.RemoteEndPoint}】");
            //发送注册包
            var packBytes = new List<byte>() { 0x1, 0x1 };
            var lenBytes = BitConverter.GetBytes(RegPack2Bytes.Length).Reverse();
            packBytes.AddRange(lenBytes);
            packBytes.AddRange(RegPack2Bytes);
            NatClient.Send(packBytes.ToArray());
            HandleLog.WriteLine($"映射成功：{NatAddress} --> {RemoteHost}:{RemoteWebPort}");
        }

        static void OnPackageReceived(object sender, PackageEventArgs<NatPackageInfo> e)
        {
            switch (e.Package.FunCode)
            {
                case 0x3:
                    {
                        HandleRequest(e);
                    }
                    break;
            }
        }

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="e"></param>
        static void HandleRequest(PackageEventArgs<NatPackageInfo> e)
        {
            Task.Run(() =>
            {
                try
                {
                    var packJson = JsonHelper.Instance.Deserialize<PackJson>(e.Package.BodyRaw);
                    var headers = packJson.Headers;
                    var contentType = headers.ContainsKey("Content-Type") ? headers["Content-Type"] : null;
                    var data = packJson.Content == null ? "" : Encoding.UTF8.GetString(packJson.Content);
                    if (!string.IsNullOrEmpty(contentType))
                    {
                        var index = contentType.IndexOf(";");
                        if (index > 0)
                        {
                            //去掉; charset=utf-8
                            contentType = contentType.Substring(0, index);
                        }
                    }
                    var res = HttpHelper.Request(packJson.Method, NatAddress + packJson.Route, data, headers: headers, contentType: contentType);
                    if (res == null)
                    {
                        HandleLog.WriteLine("服务器返回NULL");
                        return;
                    }
                    using (res)
                    {
                        using (var stream = res.Content.ReadAsStreamAsync().Result)
                        {
                            var result = DataHelper.StreamToBytes(stream);
                            var rawResult = Encoding.UTF8.GetString(result);
                            StringBuilder resp = new StringBuilder();
                            resp.Append($"{NatAddress.Split(':')[0].ToUpper()}/{res.Version} {(int)res.StatusCode} {res.StatusCode.ToString()}\r\n");
                            foreach (var item in res.Headers)
                            {
                                if (item.Key != "Transfer-Encoding")
                                {
                                    resp.Append($"{item.Key}: {string.Join(";", item.Value)}\r\n");
                                }
                            }
                            foreach (var item in res.Content.Headers)
                            {
                                resp.Append($"{item.Key}: {string.Join(";", item.Value)}\r\n");
                            }
                            if (!res.Content.Headers.Contains("Content-Length"))
                            {
                                resp.Append($"Content-Length: {result.Length}\r\n");
                            }
                            resp.Append($"Date: {DateTime.Now}\r\n");
                            resp.Append("Connection:close\r\n\r\n");

                            var response = Encoding.UTF8.GetBytes(resp.ToString()).ToList();
                            response.AddRange(result);

                            //先gzip压缩  再转为16进制字符串
                            var body = DataHelper.Compress(response.ToArray());
                            //HandleLog.WriteLine($"压缩前：{response.Count}  压缩后：{body.Length}");
                            var pack = new PackJson()
                            {
                                Host = packJson.Host,
                                UserId = packJson.UserId,
                                Content = body,
                                ResponseInfo = $"{packJson.Method} {packJson.Route} {(int)res.StatusCode} {res.StatusCode.ToString()}"
                            };
                            var json = JsonHelper.Instance.Serialize(pack);
                            var jsonBytes = Encoding.UTF8.GetBytes(json);
                            //请求头 01 03 长度(4)
                            var sendBytes = new List<byte>() { 0x1, 0x3 };
                            sendBytes.AddRange(BitConverter.GetBytes(jsonBytes.Length).Reverse());
                            sendBytes.AddRange(jsonBytes);
                            NatClient.Send(sendBytes.ToArray());
                            HandleLog.WriteLine(pack.ResponseInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    HandleLog.WriteLine($"处理请求异常：{ex}");
                }
            });
        }

        static void OnClientClosed(object sender, EventArgs e)
        {
            HandleLog.WriteLine($"连接【{NatClient.LocalEndPoint}】已关闭");
        }

        static void OnClientError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            HandleLog.WriteLine($"连接错误：{e.Exception}");
        }
    }
}
