using SuperNAT.AsyncSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Core
{
    public class TcpRequestInfo : IRequestInfo
    {
        public byte[] Raw { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
