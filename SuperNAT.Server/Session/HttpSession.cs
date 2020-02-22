using SuperNAT.AsyncSocket;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Server
{
    public class HttpSession : SocketSession
    {
        public Map Map { get; set; }
        public NatSession NatSession { get; set; }

        public override void Close()
        {
            var server = (HttpsServer)Server;
            server?.Close(this);
        }
    }
}
