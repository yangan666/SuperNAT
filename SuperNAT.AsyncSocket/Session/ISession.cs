using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    /// <summary>
    /// 会话接口
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// 客户端Socket
        /// </summary>
        Socket Socket { get; set; }
        /// <summary>
        /// 连接状态
        /// </summary>
        bool IsConnected { get; set; }
        /// <summary>
        /// Socket字节流
        /// </summary>
        Stream Stream { get; set; }
        /// <summary>
        /// Pipe管道读取缓冲区数据，数据来了直接读取，这是一个net core自带的高性能缓冲区，不用考虑数据大小，缓冲区大小会自适应
        /// </summary>
        PipeReader Reader { get; set; }
        /// <summary>
        /// Pipe管道写入数据到缓冲区，可以充当Socket的Send方法
        /// </summary>
        PipeWriter Writer { get; set; }
        /// <summary>
        /// 本机Socket节点
        /// </summary>
        EndPoint LocalEndPoint { get; set; }
        /// <summary>
        /// 远端Socket节点
        /// </summary>
        EndPoint RemoteEndPoint { get; set; }
        /// <summary>
        /// 缓冲区（UDP）
        /// </summary>
        byte[] Data { get; set; }
        /// <summary>
        /// 会话ID
        /// </summary>
        string SessionId { get; set; }
        /// <summary>
        /// 连接时间
        /// </summary>
        DateTime ConnectTime { get; set; }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">数据</param>
        void Send(byte[] data);
        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();
    }
}
