using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    public class ServerOption : ClientOptions
    {
        public int MaxConnectionsCount { get; set; } = 1000;
        public SslServerAuthenticationOptions SslServerAuthenticationOptions { get; set; }
    }
}
