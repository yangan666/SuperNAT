using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    /// <summary>
    /// 客户端配置
    /// </summary>
    public class ClientOption
    {
        /// <summary>
        /// 服务器IP地址
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 传输协议，默认TCP
        /// </summary>
        public ProtocolType ProtocolType { get; set; } = ProtocolType.Tcp;
        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 待连接队列
        /// </summary>
        public int BackLog { get; set; } = 100;
        /// <summary>
        /// TCP_NODELAY选项是用来控制是否开启Nagle算法，该算法是为了提高较慢的广域网传输效率，减小小分组的报文个数
        /// </summary>
        public bool NoDelay { get; set; } = true;
        /// <summary>
        /// 加密协议
        /// </summary>
        public SslProtocols Security { get; set; } = SslProtocols.None;
        /// <summary>
        /// 客户端加密配置
        /// </summary>
        public SslClientAuthenticationOptions SslClientAuthenticationOptions { get; set; }
        /// <summary>
        /// 最大请求（单位：字节）
        /// </summary>
        public int MaxRequestLength { get; set; } = 1024 * 1024 * 4;
        /// <summary>
        /// UDP接收缓冲区大小
        /// </summary>
        public int ReceiveBufferSize { get; set; } = 1 * 1024 * 1024 * 1024;
    }
}
