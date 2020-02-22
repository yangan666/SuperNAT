using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Client
{
    public class NatReceiveFilter : FixHeaderReceiveFilter<NatPackageInfo>
    {
        public NatReceiveFilter()
       : base(8)
        {

        }

        public override long GetBodyLengthFromHeader(ReadOnlySequence<byte> header)
        {
            var headerStr = header.ToArray().CloneRange(1, HeaderSize - 1).ToUTF8String();
            var bodyLen = Convert.ToInt32(headerStr) + 1;

            return bodyLen;
        }

        public override NatPackageInfo DecodePackage(ReadOnlySequence<byte> data)
        {
            try
            {
                var pack = data.ToArray();
                long pos = 0;
                byte head = pack[pos];
                pos += HeaderSize;
                long totalLen = pack.Length;
                string bodyStr = Encoding.UTF8.GetString(data.Slice(pos, BodySize - 1).ToArray());
                JsonData jsonData = bodyStr.FromJson<JsonData>();
                pos += BodySize;
                byte end = pack[pos - 1];

                return NatPackageInfo.OK(pack, head, totalLen, BodySize, jsonData, end);
            }
            catch (Exception ex)
            {
                return NatPackageInfo.Fail(ex.Message);
            }
        }
    }
}
