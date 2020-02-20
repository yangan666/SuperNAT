using SuperNAT.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
        public Stream Stream { get; set; }
        public PipeReader Reader { get; set; }
        public PipeWriter Writer { get; set; }

        public virtual void Close()
        {
            Socket?.Close();
        }

        private static object lockSend = new object();
        public async void Send(byte[] data)
        {
            try
            {
                await Writer.WriteAsync(data);
                await Writer.FlushAsync();
            }
            catch (Exception ex)
            {
                Close();
                HandleLog.WriteLine($"Send Error,Socket Close,{ex}");
            }
        }
    }
}
