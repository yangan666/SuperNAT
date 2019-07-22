using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common
{
    public class PackJson
    {
        public string Host { get; set; }
        public string UserId { get; set; }
        public string Method { get; set; }
        public string Route { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public byte[] Content { get; set; }
        public string ResponseInfo { get; set; }
    }
}
