using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Server
{
    public class NatSession : SocketSession
    {
        public Client Client { get; set; }
        public List<Map> MapList { get; set; }

        public override void Close()
        {
            var server = (NatServer)Server;
            server?.Close(this);
        }
    }
}
