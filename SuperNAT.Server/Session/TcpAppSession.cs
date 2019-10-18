using CSuperSocket.SocketBase;
using SuperNAT.Common;
using SuperNAT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperNAT.Server
{
    public class TcpAppSession : AppSession<TcpAppSession, TcpRequestInfo>
    {
        public TcpAppServer TcpAppServer => (TcpAppServer)AppServer;
        public string Token { get; set; }
        public string Host { get; set; }
        public string UserId { get; set; } = Guid.NewGuid().ToString();
        public string RequestInfo { get; set; }
        public DateTime? RequestTime { get; set; }
    }
}
