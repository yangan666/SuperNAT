using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Client
{
    public class NatPackageInfo : RequestInfo
    {
        public NatPackageInfo()
        {

        }

        public NatPackageInfo(bool isSuccess, string message = "")
        {
            Success = isSuccess;
            Message = message;
        }

        public static NatPackageInfo OK(byte[] raw, byte head, long total, long bodyLen, JsonData body, byte end)
        {
            return new NatPackageInfo(true)
            {
                Raw = raw,
                Head = head,
                TotalLength = total,
                BodyLength = bodyLen,
                Body = body,
                End = end
            };
        }

        public static NatPackageInfo Fail(string error)
        {
            return new NatPackageInfo(false, error);
        }
    }
}