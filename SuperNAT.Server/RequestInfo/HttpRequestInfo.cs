using CSuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Server
{
    public class HttpRequestInfo : IRequestInfo
    {
        public HttpRequestInfo(string header, string body)
        {
            Header = header;
            Body = body;
        }

        public string Key { get; set; }
        public string Header { get; set; }
        public string Body { get; set; }

        public string FirstLine { get; set; }
    }
}
