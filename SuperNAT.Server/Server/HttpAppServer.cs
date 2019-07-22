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
    public class HttpAppServer : AppServer<WebAppSession, MyRequestInfo>
    {
        public HttpAppServer()
            : base(new DefaultReceiveFilterFactory<HttpReceiveFilter, MyRequestInfo>())
        {

        }

        protected override bool ValidateClientCertificate(WebAppSession session, object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
