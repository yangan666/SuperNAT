using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Core
{
    public class TcpClientProxy : AppClient<TcpClientProxy, TcpRequestInfo>
    {
        /// <summary>
        /// 连接到外网服务器的客户端信息
        /// </summary>
        public TcpModel RemoteSession { get; set; }
        public NatClient NatClient { get; set; }
        public TcpClientProxy()
        {

        }
        public TcpClientProxy(ClientOption clientOptions) : base(null)//不需要过滤器，原样转发
        {
            InitOption(clientOptions);
        }

        public async void ConectLocalServerAsync()
        {
            OnConnected += OnClientConnected;
            OnReceived += OnPackageReceived;
            OnClosed += OnClientClosed;
            await ConnectAsync();
        }

        private void OnClientConnected(object o)
        {
            ClientManager.TcpClientProxyList.Add(this);
            HandleLog.Log($"【{RemoteSession.SessionId},{LocalEndPoint}】已连接到服务器【{RemouteEndPoint}】");
        }

        private void OnPackageReceived(object o, TcpRequestInfo tcpRequestInfo)
        {
            Task.Run(() =>
            {
                var tcpModel = tcpRequestInfo.Raw;
                //先gzip压缩  再转为16进制字符串
                var body = DataHelper.Compress(tcpRequestInfo.Raw);
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
                HandleLog.Log($"<---- {RemoteSession.SessionId} 收到报文{tcpRequestInfo.Raw.Length}字节");
            });
        }

        private void OnClientClosed(object o)
        {
            HandleLog.Log($"TcpClientProxy {LocalEndPoint}已关闭");
        }

        public void ProcessData(NatClient natClient, NatRequestInfo natRequestInfo)
        {
            try
            {
                var tcpModel = natRequestInfo.Body.Data.FromJson<TcpModel>();
                switch (natRequestInfo.Body.Action)
                {
                    case (int)TcpAction.Connect:
                        {
                            //tcp注册包  发起连接到内网服务器
                            RemoteSession = tcpModel;
                            ConectLocalServerAsync();
                        }
                        break;
                    case (int)TcpAction.TransferData:
                        {
                            //先讲16进制字符串转为byte数组  再gzip解压
                            var request = DataHelper.Decompress(tcpModel.Content);
                            //发送原始包
                            Send(request);
                            HandleLog.Log($"----> {RemoteSession.SessionId} 发送报文{request.Length}字节");
                        }
                        break;
                    case (int)TcpAction.Close:
                        {
                            //tcp连接关闭包
                            ClientManager.TcpClientProxyList.Remove(this);
                            HandleLog.Log($"本地连接【{RemoteSession.SessionId},{LocalEndPoint}】关闭成功");
                            Close();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                HandleLog.Log($"客户端处理TCP穿透业务异常，{ex}");
            }
        }
    }
}
