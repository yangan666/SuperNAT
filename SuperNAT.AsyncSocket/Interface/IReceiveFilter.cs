using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    public interface IReceiveFilter<TRequestInfo> where TRequestInfo : IRequestInfo, new()
    {
        TRequestInfo Filter(ref SequenceReader<byte> reader);
        void Reset();
    }
}
