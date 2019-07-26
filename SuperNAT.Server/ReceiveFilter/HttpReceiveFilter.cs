using CSuperSocket.Common;
using CSuperSocket.Facility.Protocol;
using CSuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Server
{
    public class HttpReceiveFilter : IReceiveFilter<MyRequestInfo>
    {
        public int LeftBufferSize { get; set; }

        public IReceiveFilter<MyRequestInfo> NextReceiveFilter { get; set; }

        public FilterState State { get; set; }

        public MyRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            rest = 0;
            var bodyBytes = new byte[length];
            Array.Copy(readBuffer, offset, bodyBytes, 0, length);
            return new MyRequestInfo(null, null, bodyBytes);
        }

        public void Reset()
        {
            
        }
    }
}
  