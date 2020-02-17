using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Client
{
    public class TcpClientProxy : SocketClient<NatPackageInfo>
    {
        /// <summary>
        /// 连接到外网服务器的客户端信息
        /// </summary>
        public TcpModel RemoteSession { get; set; }

        public NatClient NatClient { get; set; }

        public TcpClientProxy(ClientOptions clientOptions) : base(clientOptions)
        {

        }

        public void ConectLocalServer()
        {
            Task.Run(async () =>
            {
                //不加过滤器
                //NatClient.Initialize(null);
                OnConnected += OnClientConnected;
                OnReceived += OnPackageReceived;
                OnClosed += OnClientClosed;
                await ConnectAsync();
            });
        }

        private void OnClientConnected(Socket socket)
        {
            ClientHandler.TcpClientProxyList.Add(this);
            HandleLog.WriteLine($"【{RemoteSession.SessionId},{Local}】已连接到服务器【{Remote}】");
        }

        private void OnPackageReceived(Socket socket, NatPackageInfo packageInfo)
        {
            Task.Run(() =>
            {
                var tcpModel = packageInfo.Raw;
                //先gzip压缩  再转为16进制字符串
                var body = DataHelper.Compress(packageInfo.Raw);
                var pack = PackHelper.CreatePack(new JsonData()
                {
                    Type = (int)JsonType.TCP,
                    Action = (int)TcpAction.TransferData,
                    Data = new TcpModel()
                    {
                        ServerId = RemoteSession.ServerId,
                        Host = RemoteSession.Host,
                        Local = RemoteSession.Local,
                        SessionId = RemoteSession.SessionId,
                        Content = body
                    }.ToJson()
                });
                //转发给服务器
                NatClient.Send(pack);
                HandleLog.WriteLine($"<---- {RemoteSession.SessionId} 收到报文{packageInfo.Raw.Length}字节");
            });
        }

        private void OnClientClosed(Socket socket)
        {
            HandleLog.WriteLine($"TcpClientProxy {Local}已关闭");
        }

        public void ProcessData(NatClient natClient, NatPackageInfo packageInfo)
        {
            try
            {
                var tcpModel = packageInfo.Body.Data.FromJson<TcpModel>();
                switch (packageInfo.Body.Action)
                {
                    case (int)TcpAction.Connect:
                        {
                            //tcp注册包  发起连接到内网服务器
                            RemoteSession = tcpModel;
                            ConectLocalServer();
                        }
                        break;
                    case (int)TcpAction.TransferData:
                        {
                            //先讲16进制字符串转为byte数组  再gzip解压
                            var request = DataHelper.Decompress(tcpModel.Content);
                            //发送原始包
                            if (IsConnected)
                            {
                                Send(request);
                                HandleLog.WriteLine($"----> {RemoteSession.SessionId} 发送报文{request.Length}字节");
                            }
                        }
                        break;
                    case (int)TcpAction.Close:
                        {
                            //tcp连接关闭包
                            ClientHandler.TcpClientProxyList.Remove(this);
                            HandleLog.WriteLine($"本地连接【{RemoteSession.SessionId},{Local}】关闭成功");
                            Close();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"客户端处理TCP穿透业务异常，{ex}");
            }
        }
    }
}
