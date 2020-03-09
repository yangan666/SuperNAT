using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperNAT.Client
{
    public class NatClient : SocketClient<NatPackageInfo>
    {
        public Model.Client Client { get; set; }
        public NatClient(ClientOptions clientOptions) : base(clientOptions)
        {

        }

        public void ProcessData(Socket socket, NatPackageInfo packageInfo)
        {
            try
            {
                switch (packageInfo.Body.Action)
                {
                    case (int)NatAction.Connect:
                        {
                            //注册包回复
                            Client = packageInfo.Body.Data.FromJson<Model.Client>();
                            if (Client.MapList == null)
                                Client.MapList = new List<Map>();
                            HandleLog.WriteLine($"【{Client.user_name},{Client.name}】主机密钥验证成功！");
                            if (Client.MapList.Any())
                            {
                                foreach (var item in Client.MapList)
                                {
                                    HandleLog.WriteLine($"【{item.name}】映射成功：{item.local_endpoint} --> {item.remote_endpoint}");
                                }
                            }
                            else
                            {
                                HandleLog.WriteLine($"端口映射列表为空,请到管理后台创建映射！");
                            }
                        }
                        break;
                    case (int)NatAction.MapChange:
                        {
                            //Map变动
                            var map = packageInfo.Body.Data.FromJson<Map>();
                            ChangeMap(map);
                        }
                        break;
                    case (int)NatAction.ServerMessage:
                        {
                            //服务端消息
                            var msg = packageInfo.Body.Data.FromJson<ServerMessage>();
                            ClientHandler.IsReConnect = msg.ReConnect;
                            HandleLog.WriteLine(msg.Message);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"客户端处理穿透业务异常，{ex}");
            }
        }

        public void ChangeMap(Map map)
        {
            if (Client.MapList == null)
            {
                Client.MapList = new List<Map>();
            }
            switch (map.ChangeType)
            {
                case (int)ChangeMapType.新增:
                    Client.MapList.Add(map);
                    break;
                case (int)ChangeMapType.修改:
                    Client.MapList.RemoveAll(c => c.id == map.id);
                    Client.MapList.Add(map);
                    break;
                case (int)ChangeMapType.删除:
                    Client.MapList.RemoveAll(c => c.id == map.id);
                    break;
            }
            HandleLog.WriteLine($"映射{Enum.GetName(typeof(ChangeMapType), map.ChangeType)}成功：{JsonHelper.Instance.Serialize(map)}", false);
            HandleLog.WriteLine($"【{map.name}】映射{Enum.GetName(typeof(ChangeMapType), map.ChangeType)}成功：{map.local_endpoint} --> {map.remote_endpoint}");
        }
    }
}
