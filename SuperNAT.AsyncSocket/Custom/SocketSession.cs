using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    public class SocketSession : ISession
    {
        public IServer Server { get; set; }
        public Socket Socket { get; set; }
        public string Remote { get; set; }
        public string Local { get; set; }
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public DateTime ConnectTime { get; set; } = DateTime.Now;

        public virtual void Close()
        {
            Socket?.Close();
        }

        public void Send(byte[] data)
        {
            Socket?.Send(data);
        }
    }
}
