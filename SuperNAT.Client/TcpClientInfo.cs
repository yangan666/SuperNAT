using SuperNAT.Common;
using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace SuperNAT.Client
{
    public class TcpClientInfo
    {
        public TcpClientInfo(PackJson packJson, EasyClient<NatPackageInfo> natClient)
        {
            PackJson = packJson;
            NatClient = natClient;
            Init();
        }
        public PackJson PackJson { get; set; }
        public EasyClient<NatPackageInfo> NatClient { get; set; }
        public EasyClient<ClientPackageInfo> TcpClient { get; set; }
        public string LocalEndPoint { get; set; }
        public void Init()
        {
            TcpClient = new EasyClient<ClientPackageInfo>()
            {
                //Security = new SecurityOption()
                //{
                //    EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                //    AllowNameMismatchCertificate = true,
                //    AllowCertificateChainErrors = true,
                //    AllowUnstrustedCertificate = true
                //}
            };
            TcpClient.Initialize(new ClientReceiveFilter());
            TcpClient.Connected += OnClientConnected;
            TcpClient.NewPackageReceived += OnPackageReceived;
            TcpClient.Error += OnClientError;
            TcpClient.Closed += OnClientClosed;
            var arr = PackJson.Local.Split(":");
            var ip = arr[0];
            int.TryParse(arr[1], out int port);
            mark:
            var res = TcpClient.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port)).Result;
            while (!res)
            {
                HandleLog.WriteLine($"{PackJson.UserId}连接{PackJson.Local}失败,1s后重新连接...");
                Thread.Sleep(1000);
                goto mark;
            }
            LocalEndPoint = TcpClient.LocalEndPoint?.ToString();
        }

        public void OnClientConnected(object sender, EventArgs e)
        {
            HandleLog.WriteLine($"【{PackJson.UserId},{TcpClient.LocalEndPoint}】已连接到服务器【{TcpClient.Socket.RemoteEndPoint}】");
        }

        public void OnPackageReceived(object sender, PackageEventArgs<ClientPackageInfo> e)
        {
            //先gzip压缩  再转为16进制字符串
            //var body = DataHelper.Compress(e.Package.Data);
            var pack = new PackJson()
            {
                Host = PackJson.Host,
                UserId = PackJson.UserId,
                Content = e.Package.Data
            };
            var json = JsonHelper.Instance.Serialize(pack);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            //03 02 数据长度(4) 正文数据(n)   ---tcp响应包
            var sendBytes = new List<byte>() { 0x3, 0x2 };
            sendBytes.AddRange(BitConverter.GetBytes(jsonBytes.Length).Reverse());
            sendBytes.AddRange(jsonBytes);
            //转发给服务器
            NatClient.Send(sendBytes.ToArray());
            HandleLog.WriteLine($"连接【{PackJson.UserId},{PackJson.Local}】收到报文并响应{e.Package.Data.Length}字节：{DataHelper.ByteToHex(e.Package.Data)}", false);
        }

        public void OnClientClosed(object sender, EventArgs e)
        {
            HandleLog.WriteLine($"连接【{PackJson.UserId},{LocalEndPoint}】已关闭");
            //关闭外网远程连接
            CloseRemouteClient();
        }

        public void OnClientError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            HandleLog.WriteLine($"连接【{PackJson.UserId},{LocalEndPoint}】,错误：{e.Exception}");
            //关闭外网远程连接
            CloseRemouteClient();
        }

        public void CloseRemouteClient()
        {
            var pack = new PackJson()
            {
                Host = PackJson.Host,
                UserId = PackJson.UserId
            };
            var json = JsonHelper.Instance.Serialize(pack);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            //03 03 数据长度(4) 正文数据(n)   ---tcp连接关闭包
            var sendBytes = new List<byte>() { 0x3, 0x3 };
            sendBytes.AddRange(BitConverter.GetBytes(jsonBytes.Length).Reverse());
            sendBytes.AddRange(jsonBytes);
            //转发给服务器
            NatClient.Send(sendBytes.ToArray());
        }
    }
}
