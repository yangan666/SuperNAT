using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    public abstract class FixHeaderReceiveFilter<TRequestInfo> : IReceiveFilter<TRequestInfo> where TRequestInfo : IRequestInfo, new()
    {
        public FixHeaderReceiveFilter(int headerSize)
        {
            HeaderSize = headerSize;
        }

        /// <summary>
        /// 是否找到头部
        /// </summary>
        public bool FoundHeader { get; private set; }

        /// <summary>
        /// 头部长度
        /// </summary>
        public int HeaderSize { get; private set; }

        /// <summary>
        /// 数据长度
        /// </summary>
        public long BodySize { get; private set; }

        /// <summary>
        /// 总长度
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// 计算数据长度
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public abstract long GetBodyLengthFromHeader(ReadOnlySequence<byte> header);

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public TRequestInfo Filter(ref SequenceReader<byte> reader)
        {
            if (!FoundHeader)
            {
                if (reader.Length < HeaderSize)
                    return default;

                var header = reader.Sequence.Slice(0, HeaderSize);
                BodySize = GetBodyLengthFromHeader(header);

                if (BodySize < 0)
                    throw new Exception("正文长度不能小于0");

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

        /// <summary>
        /// 解析为实体
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract TRequestInfo DecodePackage(ReadOnlySequence<byte> data);

        /// <summary>
        /// 重置变量
        /// </summary>
        public void Reset()
        {
            FoundHeader = false;
            BodySize = 0;
            TotalSize = 0;
        }
    }
}
