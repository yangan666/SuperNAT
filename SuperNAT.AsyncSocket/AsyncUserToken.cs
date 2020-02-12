using SuperNAT.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    public class AsyncUserToken
    {
        /// <summary>
        /// 连接ID
        /// </summary>
        public string SessionId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>  
        /// 客户端IP地址  
        /// </summary>  
        public IPAddress IPAddress { get; set; }

        /// <summary>  
        /// 远程地址  
        /// </summary>  
        public EndPoint Remote { get; set; }

        /// <summary>  
        /// 通信SOKET  
        /// </summary>  
        public Socket Socket { get; set; }

        /// <summary>  
        /// 连接时间  
        /// </summary>  
        public DateTime ConnectTime { get; set; }

        /// <summary>  
        /// 数据缓存区  
        /// </summary>  
        public List<byte> Buffer { get; set; } = new List<byte>();

        /// <summary>
        /// 缓存区 16进制数据
        /// </summary>
        public string HexBuffer => DataHelper.ByteToHex(Buffer.ToArray());

        /// <summary>
        /// 缓存区 ASCII字符串数据
        /// </summary>
        public string BufferString => Encoding.UTF8.GetString(Buffer.ToArray());

        /// <summary>
        /// 当前接收到的数据
        /// </summary>
        public byte[] Read { get; set; }

        /// <summary>
        /// 当前接收到的16进制数据
        /// </summary>
        public string HexRead => DataHelper.ByteToHex(Read);

        /// <summary>
        /// 当前接收到的ASCII字符串数据
        /// </summary>
        public string ReadString => Encoding.UTF8.GetString(Read);
    }
}
