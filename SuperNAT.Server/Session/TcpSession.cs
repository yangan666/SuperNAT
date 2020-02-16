using SuperNAT.AsyncSocket;
using SuperNAT.Model;
using System.Collections.Generic;

namespace SuperNAT.Server
{
    public class TcpSession : SocketSession
    {
        public Map Map { get; set; }
        public NatSession NatSession { get; set; }

        public override void Close()
        {
            var server = (TcpServer)Server;
            server?.Close(this);
        }
    }
}
