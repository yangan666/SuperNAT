using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Model
{
    public class HttpModel : TcpModel
    {
        public DateTime RequestTime { get; set; }
        public string HttpVersion { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string ContentType { get; set; }
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public DateTime ResponseTime { get; set; }
    }
}
