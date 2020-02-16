using System;
using System.Net.Sockets;

namespace SuperNAT.AsyncSocket
{
    public interface ISession
    {
        /// <summary>
        /// 服务器实例
        /// </summary>
        IServer Server { get; set; }

        /// <summary>  
        /// 通信SOKET  
        /// </summary>  
        Socket Socket { get; set; }

        /// <summary>
        /// 远程连接
        /// </summary>
        public string Remote { get; set; }

        /// <summary>
        /// 本地连接
        /// </summary>
        public string Local { get; set; }

        /// <summary>
        /// 连接ID
        /// </summary>
        string SessionId { get; set; }

        /// <summary>  
        /// 连接时间  
        /// </summary>  
        DateTime ConnectTime { get; set; }

        /// <summary>
        /// 发送二进制数据
        /// </summary>
        /// <param name="data"></param>
        void Send(byte[] data);

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();
    }
}
