using CSuperSocket.SocketBase;
using SuperNAT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Server
{
    public class NatAppSession : AppSession<NatAppSession, MyRequestInfo>
    {
        public User User { get; set; }
        public List<Map> MapList { get; set; }
    }
}
