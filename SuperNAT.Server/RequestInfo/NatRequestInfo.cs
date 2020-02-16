using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Server
{
    public class NatRequestInfo : RequestInfo
    {
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
