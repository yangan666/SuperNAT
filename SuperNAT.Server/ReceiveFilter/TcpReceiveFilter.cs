using CSuperSocket.Common;
using CSuperSocket.Facility.Protocol;
using CSuperSocket.SocketBase.Protocol;
using SuperNAT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Server
{
    /// <summary>
    /// tcp过滤器
    /// </summary>
    public class TcpReceiveFilter : IReceiveFilter<TcpRequestInfo>
    {
        public int LeftBufferSize { get; set; }

        public IReceiveFilter<TcpRequestInfo> NextReceiveFilter { get; set; }

        public FilterState State { get; set; }

        public TcpRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            rest = 0;
            return new TcpRequestInfo(readBuffer.CloneRange(offset, length));
        }

        public void Reset()
        {

        }
    }
}
