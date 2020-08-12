using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    public interface ISession
    {
        Socket Socket { get; set; }
        bool IsConnected { get; set; }
        Stream Stream { get; set; }
        PipeReader Reader { get; set; }
        PipeWriter Writer { get; set; }
        EndPoint LocalEndPoint { get; set; }
        EndPoint RemoteEndPoint { get; set; }
        byte[] Data { get; set; }
        string SessionId { get; set; }
        DateTime ConnectTime { get; set; }
        void Send(byte[] data);
        void Close();
    }
}
