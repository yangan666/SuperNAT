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
    public abstract class AppSession<TSession, TRequestInfo> : ISession
        where TSession : ISession, new()
        where TRequestInfo : IRequestInfo, new()
    {
        public Socket Socket { get; set; }
        public bool IsConnected { get; set; }
        public Stream Stream { get; set; }
        public PipeReader Reader { get; set; }
        public PipeWriter Writer { get; set; }
        public EndPoint LocalEndPoint { get; set; }
        public EndPoint RemoteEndPoint { get; set; }
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public DateTime ConnectTime { get; set; }
        public byte[] Data { get; set; } = new byte[2 * 1024 * 1024];//2M缓冲区

        public void Send(byte[] data)
        {
            try
            {
                if (Socket.ProtocolType == ProtocolType.Tcp)
                {
                    lock (Stream)
                        Stream.Write(data);
                }
                else
                {
                    lock (Socket)
                        Socket.SendTo(data, RemoteEndPoint);
                }
            }
            catch (Exception ex)
            {
                Close();
                HandleLog.Log($"发送数据出错,{ex}");
            }
        }

        public void Close()
        {
            Socket.Close();
        }
    }
}
