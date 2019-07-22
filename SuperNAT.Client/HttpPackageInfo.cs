using SuperNAT.Common;
using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Client
{
    public class HttpPackageInfo : IPackageInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="header">头部</param>
        /// <param name="bodyBuffer">数据</param>
        /// <param name="allBuffer">整个数据包</param>
        public HttpPackageInfo(byte[] data)
        {
            Data = data;
            Raw = data == null ? string.Empty : Encoding.UTF8.GetString(data);
            Hex = data == null ? string.Empty : DataHelper.ByteToHex(data);
        }

        /// <summary>
        /// 服务器返回的字节数据
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 服务器返回的数据长度
        /// </summary>
        public int Length => Data?.Length ?? 0;
        public string Raw { get; set; }
        public string Hex { get; set; }
    }
}
