using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Client
{
    public class HttpReceiveFilter : IReceiveFilter<HttpPackageInfo>
    {
        public HttpReceiveFilter()
        {
            
        }

        public IReceiveFilter<HttpPackageInfo> NextReceiveFilter { get; set; }

        public FilterState State { get; set; }

        public HttpPackageInfo Filter(BufferList data, out int rest)
        {
            rest = 0;
            List<byte> bytes = new List<byte>();
            foreach (var item in data)
            {
                bytes.AddRange(item.ToList());
            }
            return new HttpPackageInfo(bytes.ToArray());
        }

        public void Reset()
        {
            
        }
    }
}
