using SuperNAT.AsyncSocket;
using SuperNAT.Core;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Core
{
    public class NatRequestInfo : IRequestInfo
    {
        public byte[] Raw { get; set; }
        public byte Head { get; set; }
        public long TotalLength { get; set; }
        public long BodyLength { get; set; }
        public JsonData Body { get; set; }
        public byte End { get; set; }

        public bool Success { get; set; }
        public string Message { get; set; }
        public NatRequestInfo()
        {

        }

        public NatRequestInfo(bool isSuccess, string message = "")
        {
            Success = isSuccess;
            Message = message;
        }

        public static NatRequestInfo OK(byte[] raw, byte head, long total, long bodyLen, JsonData body, byte end)
        {
            return new NatRequestInfo(true)
            {
                Raw = raw,
                Head = head,
                TotalLength = total,
                BodyLength = bodyLen,
                Body = body,
                End = end
            };
        }

        public static NatRequestInfo Fail(string error)
        {
            return new NatRequestInfo(false, error);
        }
    }
}
