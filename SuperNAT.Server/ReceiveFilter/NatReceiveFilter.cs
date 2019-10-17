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
    /// Nat穿透过滤器
    /// </summary>
    public class NatReceiveFilter : FixedHeaderReceiveFilter<NatRequestInfo>
    {
        public NatReceiveFilter()
        : base(6)
        {
            //地址码(1) 功能码(1) 数据长度(4) 正文数据(n)
            //01 01 数据长度(4) 正文数据(n)   ---注册包
            //01 02 数据长度(4) 正文数据(n)   ---心跳包
            //01 03 数据长度(4) 正文数据(n)   ---http响应包
            //01 04 数据长度(4) 正文数据(n)   ---Map变动
            //01 05 数据长度(4) 正文数据(n)   ---通知客户端
        }
        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            //正文数据长度
            var bytes = new byte[length];
            Array.Copy(header, offset + 2, bytes, 0, 4);
            var len = bytes[0] * 256 * 256 * 256 + bytes[1] * 256 * 256 + bytes[2] * 256 + bytes[3];
            return len;
        }

        protected override NatRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            int len = header.Count + length;
            //预先分配大小，分配多少就是多少个。预先分配的大小一定要大于等于加进去的元素数量。否则，说不定比不分配更加糟糕。
            List<byte> listData = new List<byte>() { Capacity = len };
            listData.AddRange(header.ToArray());
            var bodyBytes = new byte[length];
            Array.Copy(bodyBuffer, offset, bodyBytes, 0, length);
            listData.AddRange(bodyBytes.ToList());
            return new NatRequestInfo(header.ToArray(), bodyBytes, listData.ToArray());
        }
    }
}
