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
    /// <summary>
    /// FixedHeaderReceiveFilter过滤器
    /// </summary>
    public class NatReceiveFilter : FixedHeaderReceiveFilter<MyRequestInfo>
    {
        /// +-------+---+-------------------------------+
        /// |request| l |                               |
        /// | name  | e |    request body               |
        /// |  (2)  | n |                               |
        /// |       |(1)|                               |
        /// +-------+---+-------------------------------+
        public NatReceiveFilter()
        : base(6)
        {
            //地址码(1) 功能码(1) 数据长度(4) 正文数据(n)
            //01 01 数据长度(4) Host(n)   ---注册包
            //01 02 数据长度(4) Host(n)   ---心跳包
            //01 03 数据长度(4) 正文数据(n)   ---http响应包
        }
        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            //正文数据长度
            var bytes = header.CloneRange(offset + 2, 4);
            var len = bytes[0] * 256 * 256 * 256 + bytes[1] * 256 * 256 + bytes[2] * 256 + bytes[3];
            return len;
        }

        protected override MyRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            int len = header.Count + length;
            //预先分配大小，分配多少就是多少个。预先分配的大小一定要大于等于加进去的元素数量。否则，说不定比不分配更加糟糕。
            List<byte> listData = new List<byte>() { Capacity = len };
            listData.AddRange(header.ToArray());
            listData.AddRange(bodyBuffer.CloneRange(offset, length));
            return new MyRequestInfo(header.ToArray(), bodyBuffer.CloneRange(offset, length), listData.ToArray());
        }
    }
}
