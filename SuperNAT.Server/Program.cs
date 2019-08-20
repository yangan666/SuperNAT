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

namespace SuperNAT.Server
{
    class Program
    {
        public static string Token { get; set; } = "3d951d8b2275425887e1e9d1e53c5fa5";
        public static string CertFile = "iot3rd.p12";
        public static string CertPassword = "IoM@1234";
        static HttpAppServer WebServer { get; set; }
        static NatAppServer NATServer { get; set; }
        static void Main(string[] args)
        {
            Dapper.SimpleCRUD.SetDialect(Dapper.SimpleCRUD.Dialect.MySQL);
            HandleLog.WriteLog += (log) =>
            {
                Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss,ffff} {log}");
                Log4netUtil.Info(log);
            };
            Startup.Init();
            Task.Run(() =>
            {
                CreateHostBuilder(args).Build().Run();
            });
            WebServer = new HttpAppServer();
            bool setup = WebServer.Setup(new RootConfig()
            {
                DisablePerformanceDataCollector = true
            }, new ServerConfig()
            {
                Ip = "Any",
                Port = 10005,
                TextEncoding = "ASCII",
                MaxRequestLength = 102400,
                MaxConnectionNumber = 10000,
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
                var start = WebServer.Start();
                if (start)
                {
                    WebServer.NewSessionConnected += WebServer_NewSessionConnected;
                    WebServer.NewRequestReceived += WebServer_NewRequestReceived;
                    WebServer.SessionClosed += WebServer_SessionClosed;
                    HandleLog.WriteLine($"Web服务启动成功，监听端口：{WebServer.Config.Port}");
                }
            }
            else
            {
                HandleLog.WriteLine($"Web服务启动失败，端口：{WebServer.Config.Port}");
            }

            NATServer = new NatAppServer();
            setup = NATServer.Setup(new RootConfig()
            {
                DisablePerformanceDataCollector = true
            }, new ServerConfig()
            {
                Ip = "Any",
                Port = 10006,
                TextEncoding = "ASCII",
                MaxRequestLength = 102400,
                MaxConnectionNumber = 10000,
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
                    NATServer.NewSessionConnected += (session) =>
                    {
                        HandleLog.WriteLine($"内网客户端【{session.RemoteEndPoint}】已连接");
                    };
                    NATServer.NewRequestReceived += (session, requestInfo) =>
                    {
                        try
                        {
                            //HandleLog.WriteLine($"NAT服务收到数据：{requestInfo.Hex}");
                            switch (requestInfo.FunCode)
                            {
                                case 0x1:
                                    {
                                        //注册包
                                        var token = requestInfo.BodyRaw;
                                        using var bll = new UserBll();
                                        var user = bll.GetOne(token).Data;
                                        if (user == null)
                                        {
                                            HandleLog.WriteLine($"Token非法，关闭连接【{session.RemoteEndPoint}】");
                                            session.Close(CSuperSocket.SocketBase.CloseReason.ServerClosing);
                                            return;
                                        }
                                        var client = NATServer.GetSessions(c => c.User?.token == token).ToList();
                                        if (client?.Count > 0)
                                        {
                                            client.ForEach(c =>
                                            {
                                                c?.Close();
                                                HandleLog.WriteLine($"【{session.User.token}】连接重复，强制关闭{c.RemoteEndPoint}");
                                            });
                                            return;
                                        }
                                        session.User = user;

                                        using var mapBll = new MapBll();
                                        session.MapList = mapBll.GetMapList(token).Data ?? new List<Map>();
                                    }
                                    break;
                                case 0x2:
                                    {
                                        //心跳包
                                        HandleLog.WriteLine($"收到连接：{session.RemoteEndPoint}，用户：{session.User?.user_name}的心跳包");
                                    }
                                    break;
                                case 0x3:
                                    {
                                        //响应请求
                                        var packJson = JsonHelper.Instance.Deserialize<PackJson>(requestInfo.BodyRaw);
                                        int count = 0;
                                        mark:
                                        var webSession = WebServer.GetSessions(c => c.UserId.ToLower() == packJson.UserId.ToLower()).FirstOrDefault();
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
                                        HandleLog.WriteLine($"{packJson.ResponseInfo} {(DateTime.Now - webSession.RequestTime).Value.TotalSeconds}s");
                                        //webSession?.Close();
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            HandleLog.WriteLine($"webSession响应请求异常：{ex}");
                        }
                    };
                    NATServer.SessionClosed += (session, reasion) =>
                    {
                        HandleLog.WriteLine($"内网客户端【{session.RemoteEndPoint}】已下线");
                    };
                    HandleLog.WriteLine($"NAT服务启动成功，监听端口：{NATServer.Config.Port}");
                }
            }
            else
            {
                HandleLog.WriteLine($"NAT服务启动失败，端口：{NATServer.Config.Port}");
            }

            Console.ReadKey();
        }

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
                            options.Listen(IPAddress.Any, 8088);
                            //options.Listen(IPAddress.Loopback, 5001, listenOptions =>
                            //{
                            //    listenOptions.UseHttps("testCert.pfx", "testPassword");
                            //});
                        })
                        .UseContentRoot(contentRoot)  // set content root
                        .UseWebRoot(webRoot);         // set web root
            });

        private static void WebServer_NewSessionConnected(WebAppSession session)
        {
            //HandleLog.WriteLine($"客户端【{session.SessionID}】已连接");
        }

        private static void WebServer_NewRequestReceived(WebAppSession session, MyRequestInfo requestInfo)
        {
            Task.Run(() =>
            {
                try
                {
                    if (session.RequestTime == null)
                    {
                        session.RequestTime = DateTime.Now;
                    }
                    session.RequestByteList.AddRange(requestInfo.Data);
                    var headers = new Dictionary<string, string>();
                    var firstLine = string.Empty;
                    var result = string.Empty;
                    var content = string.Empty;
                    int contentLength = 0, contentLen = 0, headerIndex = 0;
                    string method = string.Empty;
                    string route = string.Empty;
                    var sss = Encoding.UTF8.GetString(session.RequestByteList.ToArray());
                    headerIndex = DataHelper.BytesIndexOf(session.RequestByteList.ToArray(), Encoding.UTF8.GetBytes("\r\n\r\n"));
                    if (headerIndex <= 0)
                    {
                        return;
                    }
                    result = Encoding.UTF8.GetString(session.RequestByteList.CloneRange(0, headerIndex));
                    //找到header结束标识 先判断有没有Body
                    string[] headerStr = result.Substring(0, headerIndex).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < headerStr.Length; i++)
                    {
                        string[] temp = headerStr[i].Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                        if (temp.Length == 2)
                        {
                            if (headers.ContainsKey(temp[0]))
                            {
                                headers[temp[0]] = temp[1];
                            }
                            else
                            {
                                headers.Add(temp[0], temp[1]);
                            }
                        }
                        else
                        {
                            session.RequestInfo = firstLine = string.Join("", temp);
                            var headItems = firstLine.Split(' ');
                            method = headItems[0];
                            route = headItems[1].Trim();
                        }
                    }
                    //if (header.ContainsKey("Transfer-Encoding") && header["Transfer-Encoding"].ToLower().Contains("chunked"))
                    //{
                    //    var str = Encoding.UTF8.GetString(requestByteList.ToArray());
                    //    goto mark;
                    //}
                    if (headers.ContainsKey("Content-Length"))
                    {
                        contentLength = Convert.ToInt32(headers["Content-Length"]);
                    }
                    contentLen = session.RequestByteList.Count - headerIndex - 4;
                    if (contentLen < contentLength)
                    {
                        return;
                    }
                    else if (contentLen == contentLength)
                    {
                        //HandleLog.WriteLine($"长度是对的，header给的长度为：{contentLength}，接收长度为：{contentLen}");
                    }
                    else
                    {
                        //HandleLog.WriteLine($"长度超出了，header给的长度为：{contentLength}，接收长度为：{contentLen}");
                    }

                    //没有Host直接返回
                    if (!headers.ContainsKey("Host"))
                    {
                        HandleLog.WriteLine("Host不存在，放弃处理！");
                        session?.Close();
                        return;
                    }
                    //转发请求
                    var host = headers["Host"];
                    var natSession = NATServer.GetSessions(c => c.MapList?.Any(m => m.remote == host) ?? false).FirstOrDefault();
                    if (natSession == null)
                    {
                        session?.Close();
                        HandleLog.WriteLine("Nat客户端连接不存在，放弃处理！");
                        return;
                    }
                    var request = session.RequestByteList.ToString();
                    var pack = new PackJson()
                    {
                        Host = host,
                        UserId = session.UserId,
                        Method = method,
                        Route = route,
                        Headers = headers,
                        Content = contentLen > 0 ? session.RequestByteList.CloneRange(headerIndex + 4, session.RequestByteList.Count - headerIndex - 4) : null
                    };
                    var json = JsonHelper.Instance.Serialize(pack);
                    var jsonBytes = Encoding.UTF8.GetBytes(json);
                    //请求头 01 03 长度(4)
                    var sendBytes = new List<byte>() { 0x1, 0x3 };
                    sendBytes.AddRange(BitConverter.GetBytes(jsonBytes.Length).Reverse());
                    sendBytes.AddRange(jsonBytes);
                    natSession.Send(sendBytes.ToArray(), 0, sendBytes.Count);
                    session.RequestByteList.Clear();
                }
                catch (Exception ex)
                {
                    HandleLog.WriteLine($"处理发生异常：{ex}");
                }
            });
        }

        private static void WebServer_SessionClosed(WebAppSession session, CSuperSocket.SocketBase.CloseReason value)
        {
            //HandleLog.WriteLine($"客户端【{session.SessionID}】已下线：{value}");
        }
    }
}
