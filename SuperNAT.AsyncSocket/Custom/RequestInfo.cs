using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.AsyncSocket
{
    public class RequestInfo : IRequestInfo
    {
        public byte[] Raw { get; set; }
        public byte Head { get; set; }
        public long TotalLength { get; set; }
        public long BodyLength { get; set; }
        public JsonData Body { get; set; }
        public byte End { get; set; }

        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
