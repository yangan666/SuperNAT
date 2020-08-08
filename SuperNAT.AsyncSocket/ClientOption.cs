using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    public class ClientOption
    {
        public string Ip { get; set; }

        public int Port { get; set; }

        public ProtocolType ProtocolType { get; set; } = ProtocolType.Tcp;

        public string Path { get; set; }

        public int BackLog { get; set; }

        public bool NoDelay { get; set; } = true;

        public SslProtocols Security { get; set; }

        public SslClientAuthenticationOptions SslClientAuthenticationOptions { get; set; }

        public int MaxRequestLength { get; set; } = 1024 * 1024 * 4;

        public int ReceiveBufferSize { get; set; } = 1024 * 4;
    }
}
