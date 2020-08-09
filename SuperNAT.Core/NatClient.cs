using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using SuperNAT.Core;
using SuperNAT.Model;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperNAT.Core
{
    public class NatClient : AppClient<NatClient, NatRequestInfo>
    {
        public Client Client { get; set; }
        public NatClient()
        {

        }
        public NatClient(ClientOption clientOption) : base(new NatReceiveFilter())
        {
            InitOption(clientOption);
        }

        public void ProcessData(NatRequestInfo natRequestInfo)
        {
            try
            {
                switch (natRequestInfo.Body.Action)
                {
                    case (int)NatAction.Connect:
                        {
                            //注册包回复
                            Client = natRequestInfo.Body.Data.FromJson<Client>();
                            if (Client.MapList == null)
                                Client.MapList = new List<Map>();
                            HandleLog.Log($"【{Client.user_name},{Client.name}】主机密钥验证成功！");
                            if (Client.MapList.Any())
                            {
                                foreach (var item in Client.MapList)
                                {
                                    HandleLog.Log($"【{item.name}】映射成功：{item.local_endpoint} --> {item.remote_endpoint}");
                                }
                            }
                            else
                            {
                                HandleLog.Log($"端口映射列表为空,请到管理后台创建映射！");
                            }
                        }
                        break;
                    case (int)NatAction.MapChange:
                        {
                            //Map变动
                            var map = natRequestInfo.Body.Data.FromJson<Map>();
                            ChangeMap(map);
                        }
                        break;
                    case (int)NatAction.ServerMessage:
                        {
                            //服务端消息
                            var msg = natRequestInfo.Body.Data.FromJson<ServerMessage>();
                            ClientManager.IsReConnect = msg.ReConnect;
                            HandleLog.Log(msg.Message);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                HandleLog.Log($"客户端处理穿透业务异常，{ex}");
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
            HandleLog.Log($"映射{Enum.GetName(typeof(ChangeMapType), map.ChangeType)}成功：{JsonHelper.Instance.Serialize(map)}", false);
            HandleLog.Log($"【{map.name}】映射{Enum.GetName(typeof(ChangeMapType), map.ChangeType)}成功：{map.local_endpoint} --> {map.remote_endpoint}");
        }
    }
}
