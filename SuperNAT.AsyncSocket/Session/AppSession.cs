using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.AsyncSocket
{
    /// <summary>
    /// 连接会话
    /// </summary>
    /// <typeparam name="TSession"></typeparam>
    /// <typeparam name="TRequestInfo"></typeparam>
    public abstract class AppSession<TSession, TRequestInfo> : ISession
        where TSession : ISession, new()
        where TRequestInfo : IRequestInfo, new()
    {
        /// <summary>
        /// Session所属服务
        /// </summary>
        public IServer<TSession, TRequestInfo> AppServer { get; set; }
        /// <summary>
        /// 客户端Socket
        /// </summary>
        public Socket Socket { get; set; }
        /// <summary>
        /// 连接状态
        /// </summary>
        public bool IsConnected { get; set; }
        /// <summary>
        /// Socket字节流
        /// </summary>
        public Stream Stream { get; set; }
        /// <summary>
        /// Pipe管道读取缓冲区数据，数据来了直接读取，这是一个net core自带的高性能缓冲区，不用考虑数据大小，缓冲区大小会自适应
        /// </summary>
        public PipeReader Reader { get; set; }
        /// <summary>
        /// Pipe管道写入数据到缓冲区，可以充当Socket的Send方法
        /// </summary>
        public PipeWriter Writer { get; set; }
        /// <summary>
        /// 本机Socket节点
        /// </summary>
        public EndPoint LocalEndPoint { get; set; }
        /// <summary>
        /// 远端Socket节点
        /// </summary>
        public EndPoint RemoteEndPoint { get; set; }
        /// <summary>
        /// 会话ID
        /// </summary>
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// 连接时间
        /// </summary>
        public DateTime ConnectTime { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// 缓冲区（UDP）
        /// </summary>
        public byte[] Data { get; set; } = new byte[10 * 1024];//10KB缓冲区

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data">数据</param>
        public void Send(byte[] data)
        {
            try
            {
                //发送对象（远程主机）
                EndPoint sendEndPoint = null;
                if (Socket.ProtocolType == ProtocolType.Tcp)
                {
                    lock (Stream)
                        Stream.Write(data);
                    sendEndPoint = Socket.RemoteEndPoint;
                }
                else
                {
                    lock (Socket)
                        Socket.SendTo(data, RemoteEndPoint);
                    sendEndPoint = RemoteEndPoint;
                }
                //LogHelper.Info($"[{Socket.ProtocolType.ToString().ToUpper()}]向{sendEndPoint}发送数据【{data.ToHex()}】,共{data.Length}字节");
            }
            catch (Exception ex)
            {
                Close();
                LogHelper.Error($"发送数据出错,{ex}");
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            Socket.Close();
        }
    }
}
