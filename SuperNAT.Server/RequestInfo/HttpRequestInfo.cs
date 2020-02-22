using SuperNAT.AsyncSocket;
using System;
using System.Collections.Generic;
using System.Text;
using SuperNAT.Common;
using SuperNAT.Model;

namespace SuperNAT.Server
{
    public class HttpRequestInfo : IRequestInfo
    {
        public byte[] Raw { get; set; }

        public string HttpVersion { get; set; }

        public string Method { get; set; }

        public string BaseUrl { get; set; }

        public string ClientIP { get; set; }

        public string Path { get; set; }

        public string QueryString { get; set; }

        public string Url { get; set; }

        public Dictionary<string, string> Headers { get; private set; } = new Dictionary<string, string>();

        public string ContentType { get; set; }

        public byte[] Body { get; set; }

        public int ContentLength { get; set; }

        public FilterStatus FilterStatus { get; set; } = FilterStatus.None;

        public bool Success { get; set; }

        public string Message { get; set; }
    }
}
