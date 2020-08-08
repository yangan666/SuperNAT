using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.AsyncSocket
{
    public interface IClient<TSession, TRequestInfo> 
        where TSession : ISession, new() 
        where TRequestInfo : IRequestInfo, new()
    {
        TSession Session { get; set; }
        Socket Socket { get; set; }
        Task<bool> ConnectAsync(string ip, int port);
        void Send(byte[] data);
        void SendTo(byte[] data, EndPoint endPoint);
        Action<TSession> OnConnected { get; set; }
        Action<TSession, TRequestInfo> OnReceived { get; set; }
        Action<TSession> OnClosed { get; set; }
        void Close();
    }
}
