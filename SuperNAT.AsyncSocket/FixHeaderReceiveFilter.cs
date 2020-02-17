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
