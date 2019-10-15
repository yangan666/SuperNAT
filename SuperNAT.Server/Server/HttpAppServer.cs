using CSuperSocket.SocketBase;
using CSuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Server
{
    public class HttpAppServer : AppServer<WebAppSession, HttpRequestInfo>
    {
        public HttpAppServer()
            : base(new DefaultReceiveFilterFactory<HttpReceiveFilter, HttpRequestInfo>())
        {

        }
    }
}
