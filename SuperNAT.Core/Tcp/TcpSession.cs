using SuperNAT.AsyncSocket;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Core
{
    public class TcpSession : AppSession<TcpSession, TcpRequestInfo>
    {
        public Map Map { get; set; }
        public NatSession NatSession { get; set; }
    }
}
