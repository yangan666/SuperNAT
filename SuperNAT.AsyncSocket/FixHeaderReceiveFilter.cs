using SuperNAT.Common;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    public abstract class FixHeaderReceiveFilter<TRequestInfo> : IReceiveFilter<TRequestInfo>
        where TRequestInfo : IRequestInfo, new()
    {
        //协议 01:nat 02:http 03:tcp 04:udp
        //协议(1) 功能码(1) 数据长度(4) 正文数据(n)

        //nat
        //01 01 数据长度(4) 正文数据(n)   ---注册包
        //01 02 数据长度(4) 正文数据(n)   ---心跳包
        //01 03 数据长度(4) 正文数据(n)   ---Map变动
        //01 04 数据长度(4) 正文数据(n)   ---服务器推送消息（成功/失败信息）

        //http
        //02 01 数据长度(4) 正文数据(n)   ---http响应包

        //tcp
        //03 01 数据长度(4) 正文数据(n)   ---tcp连接注册包
        //03 02 数据长度(4) 正文数据(n)   ---tcp响应包
        //03 03 数据长度(4) 正文数据(n)   ---tcp连接关闭包

        //new:
        //协议 01:nat 02:http 03:tcp 04:udp
        //帧头(01H)  数据长度(7) 正文数据(n) 帧尾(04H)


        public bool FoundHeader { get; private set; }
        public int HeaderSize { get; private set; }
        public long BodySize { get; private set; }
        private long TotalSize { get; set; }

        public FixHeaderReceiveFilter(int headerSize)
        {
            HeaderSize = headerSize;
        }

        public abstract long GetBodyLengthFromHeader(ReadOnlySequence<byte> header);

        public TRequestInfo Filter(ref SequenceReader<byte> reader)
        {
            if (!FoundHeader)
            {
                if (reader.Length < HeaderSize)
                    return default;

                var header = reader.Sequence.Slice(0, HeaderSize);
                BodySize = GetBodyLengthFromHeader(header);

                if (BodySize < 0)
                    throw new Exception("Failed to get body length from the package header.");

                if (BodySize == 0)
                    return DecodePackage(header);

                FoundHeader = true;
                TotalSize = HeaderSize + BodySize;
            }

            var totalSize = TotalSize;

            if (reader.Length < totalSize)
                return default;

            var pack = reader.Sequence.Slice(0, totalSize);

            try
            {
                return DecodePackage(pack);
            }
            finally
            {
                reader.Advance(totalSize);
                Reset();
            }
        }

        public abstract TRequestInfo DecodePackage(ReadOnlySequence<byte> data);

        public void Reset()
        {
            FoundHeader = false;
            BodySize = 0;
            TotalSize = 0;
        }
    }
}
