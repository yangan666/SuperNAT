using SuperNAT.Common;
using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Client
{
    public class NatPackageInfo : IPackageInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="header">头部</param>
        /// <param name="bodyBuffer">数据</param>
        /// <param name="allBuffer">整个数据包</param>
        public NatPackageInfo(byte[] data)
        {
            Header = data.CloneRange(0, 6);
            Body = data.CloneRange(6, data.Length - 6);
            Data = data;
            BodyRaw = Body == null ? string.Empty : Encoding.UTF8.GetString(Body);
            Raw = data == null ? string.Empty : Encoding.UTF8.GetString(data);
            Hex = data == null ? string.Empty : DataHelper.ByteToHex(data);
            FunCode = Header == null ? (byte)0x0 : Header[1];
        }

        public byte[] Header { get; set; }

        /// <summary>
        /// 服务器返回的字节数据
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 服务器返回的数据长度
        /// </summary>
        public int Length => Data?.Length ?? 0;

        public byte[] Body { get; set; }
        public string BodyRaw { get; set; }
        public string Raw { get; set; }
        public string Hex { get; set; }
        public byte FunCode { get; set; }
    }
}
