using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Model
{
    public class ServerMessage
    {
        public byte Type { get; set; } = (byte)ServerMessageType.Error;
        public string Message { get; set; }
        /// <summary>
        /// 异常消息比如主机密钥被占用或者无效则通知客户端不重连
        /// </summary>
        public bool ReConnect { get; set; } = false;
    }
}
