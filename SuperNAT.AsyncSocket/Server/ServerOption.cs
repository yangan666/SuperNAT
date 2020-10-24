using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    /// <summary>
    /// 服务端配置
    /// </summary>
    public class ServerOption : ClientOption
    {
        /// <summary>
        /// 最大连接数
        /// </summary>
        public int MaxConnectionsCount { get; set; } = 1000;
        /// <summary>
        /// 服务端SSL加密配置
        /// </summary>
        public SslServerAuthenticationOptions SslServerAuthenticationOptions { get; set; }
    }
}
