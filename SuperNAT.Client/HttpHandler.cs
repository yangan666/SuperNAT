using log4net;
using log4net.Config;
using log4net.Repository;
using SuperNAT.Common;
using SuperNAT.Common.Models;
using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperNAT.Client
{
    public class HttpHandler
    {
        public static EasyClient<NatPackageInfo> NatClient { get; set; }
        public static string Secret { get; set; } = AppConfig.GetSetting("Secret");
        public static string ServerUrl { get; set; } = AppConfig.GetSetting("ServerUrl");
        public static string ServerPort { get; set; } = AppConfig.GetSetting("ServerPort");
        public static int NatPort { get; set; } = Convert.ToInt32(AppConfig.GetSetting("NatPort"));
        public static byte[] RegPack => Encoding.UTF8.GetBytes(Secret);
        public static List<Map> MapList { get; set; }
        public static ILoggerRepository Repository { get; set; }
        public static Thread reConnectThread, heartThread;


        public void Start()
        {
            try
            {
                Repository = LogManager.CreateRepository("NETCoreRepository");
                XmlConfigurator.Configure(Repository, new FileInfo("log4net.config"));
                Log4netUtil.LogRepository = Repository;//类库中定义的静态变量
                HandleLog.WriteLog += (log, isPrint) =>
                {
                    if (isPrint)
                    {
                        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {log}");
                    }
                    Log4netUtil.Info(log);
                };
                MapList = GetMapList(Secret)?.Data;
                if (MapList?.Any() ?? false)
                {
                    ConnectNatServer();

                    reConnectThread = new Thread(ReConnect) { IsBackground = true };
                    reConnectThread.Start();

                    heartThread = new Thread(SendHeart) { IsBackground = true };
                    heartThread.Start();
                }
                else
                {
                    HandleLog.WriteLine($"端口映射列表为空！");
                }
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"{ex}");
            }
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
                    var map = MapList.Find(c => c.remote == packJson.Host);
                    if (map == null)
                    {
                        HandleLog.WriteLine($"映射不存在，外网访问地址：{packJson.Host}");
                        return;
                    }
                    var res = HttpHelper.Request(packJson.Method, $"{map.protocol}://{map.local}{packJson.Route}", data, headers: headers, contentType: contentType);
                    if (res == null)
                    {
                        HandleLog.WriteLine("服务器返回NULL");
                        return;
                    }
                    using (res)
                    {
                        using var stream = res.Content.ReadAsStreamAsync().Result;
                        var result = DataHelper.StreamToBytes(stream);
                        var rawResult = Encoding.UTF8.GetString(result);
                        StringBuilder resp = new StringBuilder();
                        resp.Append($"{map.protocol.ToUpper()}/{res.Version} {(int)res.StatusCode} {res.StatusCode.ToString()}\r\n");
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
                        if (packJson.Method.ToUpper() == "OPTIONS")
                        {
                            resp.Append("Access-Control-Allow-Credentials: true\r\n");
                            if (packJson.Headers.ContainsKey("Access-Control-Request-Headers"))
                            {
                                resp.Append($"Access-Control-Allow-Headers: {packJson.Headers["Access-Control-Request-Headers"]}\r\n");
                            }
                            resp.Append("Access-Control-Allow-Methods: *\r\n");
                            resp.Append($"Access-Control-Allow-Origin: {packJson.Headers["Origin"]}\r\n");
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
                        var pack = new PackJson()
                        {
                            Host = packJson.Host,
                            UserId = packJson.UserId,
                            Content = body,
                            ResponseInfo = $"{map.name} {packJson.Method} {packJson.Route} {(int)res.StatusCode} {res.StatusCode.ToString()}"
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
                catch (Exception ex)
                {
                    HandleLog.WriteLine($"处理请求异常：{ex}");
                }
            });
        }

        static ReturnResult<List<Map>> GetMapList(string secret)
        {
            var res = new ReturnResult<List<Map>>();

            try
            {
                var response = HttpHelper.HttpRequest("POST", $"http://{ServerUrl}:{ServerPort}/Api/Map/GetMapList?secret={secret}");
                if (!string.IsNullOrEmpty(response))
                {
                    res = JsonHelper.Instance.Deserialize<ReturnResult<List<Map>>>(response);
                }
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"获取端口映射列表失败：{ex}");
            }

            return res;
        }

        static void ConnectNatServer()
        {
            try
            {
                HandleLog.WriteLine($"正在连接服务器...");
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
                //解析主机名
                IPHostEntry ipInfo = Dns.GetHostEntry(ServerUrl);
                var serverIp = ipInfo.AddressList.Any() ? ipInfo.AddressList[0].ToString() : throw new Exception($"域名【{ServerUrl}】无法解析");
                //连接NAT转发服务
                var res = NatClient.ConnectAsync(new IPEndPoint(IPAddress.Parse(serverIp), NatPort)).Result;
                if (!res)
                {
                    Thread.Sleep(2000);
                    ConnectNatServer();
                }
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"连接服务器失败：{ex}");
                Thread.Sleep(1000);
                ConnectNatServer();
            }
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
                Thread.Sleep(30000);
                if (NatClient.IsConnected)
                {
                    //发送心跳包
                    var packBytes = new List<byte>() { 0x1, 0x2 };
                    var lenBytes = BitConverter.GetBytes(RegPack.Length).Reverse();
                    packBytes.AddRange(lenBytes);
                    packBytes.AddRange(RegPack);
                    NatClient.Send(packBytes.ToArray());
                }
            }
        }

        static void OnClientConnected(object sender, EventArgs e)
        {
            //HandleLog.WriteLine($"【{NatClient.LocalEndPoint}】已连接到服务器【{NatClient.Socket.RemoteEndPoint}】");
            //发送注册包
            var packBytes = new List<byte>() { 0x1, 0x1 };
            var lenBytes = BitConverter.GetBytes(RegPack.Length).Reverse();
            packBytes.AddRange(lenBytes);
            packBytes.AddRange(RegPack);
            NatClient.Send(packBytes.ToArray());
        }

        static void OnPackageReceived(object sender, PackageEventArgs<NatPackageInfo> e)
        {
            switch (e.Package.FunCode)
            {
                case 0x1:
                    {
                        //注册包回复
                        foreach (var item in MapList)
                        {
                            HandleLog.WriteLine($"{item.name}映射成功：{item.local} --> {item.remote}");
                        }
                    }
                    break;
                case 0x3:
                    {
                        //http请求
                        HandleRequest(e);
                    }
                    break;
                case 0x4:
                    {
                        //Map变动
                        var map = JsonHelper.Instance.Deserialize<Map>(e.Package.BodyRaw);
                        ChangeMap(map);
                    }
                    break;
            }
        }

        static void OnClientClosed(object sender, EventArgs e)
        {
            HandleLog.WriteLine($"连接{NatClient.LocalEndPoint}已关闭");
        }

        static void OnClientError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            HandleLog.WriteLine($"连接错误：{e.Exception}");
        }

        static void ChangeMap(Map map)
        {
            if (MapList == null)
            {
                MapList = new List<Map>();
            }
            switch (map.ChangeType)
            {
                case (int)ChangeMapType.新增:
                    MapList.Add(map);
                    break;
                case (int)ChangeMapType.修改:
                    var item = MapList.Find(c => c.id == map.id);
                    item = map;
                    break;
                case (int)ChangeMapType.删除:
                    MapList.RemoveAll(c => c.id == map.id);
                    break;
            }
            HandleLog.WriteLine($"映射{Enum.GetName(typeof(ChangeMapType), map.ChangeType)}成功：{JsonHelper.Instance.Serialize(map)}");
        }
    }
}
