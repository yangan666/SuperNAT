//using CSuperSocket.SocketBase;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SuperNAT.Server
//{
//    public class WebAppSession : AppSession<WebAppSession, HttpRequestInfo>
//    {
//        public HttpAppServer HttpAppServer => (HttpAppServer)AppServer;
//        public string Token { get; set; }
//        public string Host { get; set; }
//        public string UserId { get; set; } = Guid.NewGuid().ToString();
//        public string RequestInfo { get; set; }
//        public DateTime? RequestTime { get; set; }
//    }
//}
