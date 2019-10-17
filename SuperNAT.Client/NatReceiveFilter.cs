using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Client
{
    public class NatReceiveFilter : FixedHeaderReceiveFilter<NatPackageInfo>
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

        protected override int GetBodyLengthFromHeader(IBufferStream bufferStream, int length)
        {
            List<byte> bytes = new List<byte>();
            foreach (var item in bufferStream.Buffers)
            {
                bytes.AddRange(item.ToList());
            }
            byte[] header = bytes.ToArray().CloneRange(2, 4);
            //正文数据长度
            var len = header[0] * 256 * 256 * 256 + header[1] * 256 * 256 + header[2] * 256 + header[3];
            return len;
        }

        public override NatPackageInfo ResolvePackage(IBufferStream bufferStream)
        {
            List<byte> bytes = new List<byte>();
            foreach (var item in bufferStream.Buffers)
            {
                bytes.AddRange(item.ToList());
            }
            return new NatPackageInfo(bytes.ToArray());
        }
    }
}
