using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Model
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
        public string Data { get; set; }
    }
}
