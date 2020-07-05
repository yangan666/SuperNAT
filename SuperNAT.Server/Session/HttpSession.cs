using SuperNAT.AsyncSocket;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Server
{
    public class HttpSession : SocketSession
    {
        public Map Map { get; set; }
        public NatSession NatSession { get; set; }
        public HttpRequestInfo RequestInfo { get; set; }

        public override void Close()
        {
            var server = (HttpsServer)Server;
            server?.Close(this);
        }

        public void Write(string msg)
        {
            var response = new HttpResponse()
            {
                Status = 200,
                ContentType = "text/html",
                Body = Encoding.UTF8.GetBytes(msg)
            };
            //把处理信息返回到客户端
            Send(response.Write());
        }
    }
}
