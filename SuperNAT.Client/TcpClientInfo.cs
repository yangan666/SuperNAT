using SuperNAT.Common;
using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

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
        public void Init()
        {
            TcpClient = new EasyClient<ClientPackageInfo>
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
            var res = TcpClient.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port)).Result;
            HandleLog.WriteLine($"连接{PackJson.Local}{(res ? "成功" : "失败")}");
        }

        public void OnClientConnected(object sender, EventArgs e)
        {
            HandleLog.WriteLine($"【{TcpClient.LocalEndPoint}】已连接到服务器【{TcpClient.Socket.RemoteEndPoint}】");
        }

        public void OnPackageReceived(object sender, PackageEventArgs<ClientPackageInfo> e)
        {
            //先gzip压缩  再转为16进制字符串
            var body = DataHelper.Compress(e.Package.Data);
            var pack = new PackJson()
            {
                Host = PackJson.Host,
                UserId = PackJson.UserId,
                Content = body
            };
            var json = JsonHelper.Instance.Serialize(pack);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            //03 02 数据长度(4) 正文数据(n)   ---tcp响应包
            var sendBytes = new List<byte>() { 0x3, 0x2 };
            sendBytes.AddRange(BitConverter.GetBytes(jsonBytes.Length).Reverse());
            sendBytes.AddRange(jsonBytes);
            //转发给服务器
            NatClient.Send(sendBytes.ToArray());
            HandleLog.WriteLine($"TCP响应{e.Package.Data.Length}字节");
        }

        public void OnClientClosed(object sender, EventArgs e)
        {
            HandleLog.WriteLine($"连接{TcpClient.LocalEndPoint}已关闭");
        }

        public void OnClientError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            HandleLog.WriteLine($"连接错误：{e.Exception}");
        }
    }
}
