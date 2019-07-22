using CSuperSocket.SocketBase.Protocol;
using SuperNAT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Server
{
    public class MyRequestInfo : IRequestInfo
    {
        public MyRequestInfo(byte[] header, byte[] body, byte[] data)
        {
            Header = header;
            Body = body;
            Data = data;
            BodyRaw = body == null ? string.Empty : Encoding.UTF8.GetString(body);
            Raw = data == null ? string.Empty : Encoding.UTF8.GetString(data);
            Hex = data == null ? string.Empty : DataHelper.ByteToHex(data);
            FunCode = header == null ? (byte)0x0 : header[1];
        }
        public string Key { get; set; }
        public byte[] Header { get; set; }
        public byte[] Body { get; set; }
        public byte[] Data { get; set; }
        public string BodyRaw { get; set; }
        public string Raw { get; set; }
        public string Hex { get; set; }
        public byte FunCode { get; set; }
    }
}
