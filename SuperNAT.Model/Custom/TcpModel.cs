using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Model
{
    public class TcpModel
    {
        public string ServerId { get; set; }
        public string Host { get; set; }
        public string Local { get; set; }
        public string SessionId { get; set; }
        public byte[] Content { get; set; }
    }
}
