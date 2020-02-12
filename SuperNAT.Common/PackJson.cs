using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common
{
    public class JsonData
    {
        /// <summary>
        /// 01:nat 02:http 03:websocket 04:tcp 05:udp
        /// </summary>
        public byte Type { get; set; }

        /// <summary>
        /// 指令类型
        /// </summary>
        public byte Action { get; set; }

        /// <summary>
        /// 数据实体
        /// </summary>
        public object Body { get; set; }
    }

    public enum JsonType
    {
        NAT = 0x01,
        HTTP = 0x02,
        WEBSOCKET = 0x03,
        TCP = 0x04,
        UDP = 0x05
    }

    public class PackJson
    {
        public string Host { get; set; }
        public string Local { get; set; }
        public string UserId { get; set; }
        public string Method { get; set; }
        public string Route { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public byte[] Content { get; set; }
        public string ResponseInfo { get; set; }
    }
}
