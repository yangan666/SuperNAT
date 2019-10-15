using CSuperSocket.SocketBase;
using CSuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Server
{
    public class NatAppServer : AppServer<NatAppSession, NatRequestInfo>
    {
        public NatAppServer()
           : base(new DefaultReceiveFilterFactory<NatReceiveFilter, NatRequestInfo>())
        {

        }
    }
}
