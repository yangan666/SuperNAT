using SuperNAT.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    public class FixHeaderReceiveFilter
    {
        //协议 01:nat 02:http 03:tcp 04:udp
        //协议(1) 功能码(1) 数据长度(4) 正文数据(n)

        //nat
        //01 01 数据长度(4) 正文数据(n)   ---注册包
        //01 02 数据长度(4) 正文数据(n)   ---心跳包
        //01 03 数据长度(4) 正文数据(n)   ---Map变动
        //01 04 数据长度(4) 正文数据(n)   ---服务器推送消息（成功/失败信息）

        //http
        //02 01 数据长度(4) 正文数据(n)   ---http响应包

        //tcp
        //03 01 数据长度(4) 正文数据(n)   ---tcp连接注册包
        //03 02 数据长度(4) 正文数据(n)   ---tcp响应包
        //03 03 数据长度(4) 正文数据(n)   ---tcp连接关闭包

        //new:
        //协议 01:nat 02:http 03:tcp 04:udp
        //帧头(01H)  数据长度(7) 正文数据(n) 校验和(4) 帧尾(04H)


        private bool _foundHeader;
        private readonly int _headerSize;
        private int _totalSize;

        public FixHeaderReceiveFilter(int headerSize)
        {
            _headerSize = headerSize;
        }

        public int GetBodyLengthFromHeader(byte[] header)
        {
            var headerStr = header.ToHex();
            var bodyLen = Convert.ToInt32(headerStr);

            return bodyLen;
        }

        public JsonData ResolveRequestInfo(byte[] header, byte[] bodyBuffer, int offset, int length)
        {
            //if (!_foundHeader)
            //{
            //    if (header.Length < _headerSize)
            //        return null;

            //    var bodyLength = GetBodyLengthFromHeader(header);

            //    if (bodyLength < 0)
            //        throw new Exception("Failed to get body length from the package header.");

            //    if (bodyLength == 0)
            //        return null;

            //    _foundHeader = true;
            //    _totalSize = _headerSize + bodyLength;
            //}

            //var totalSize = _totalSize;

            //if (reader.Length < totalSize)
            //    return null;

            //var pack = reader.Sequence.Slice(0, totalSize);

            //try
            //{
            //    return DecodePackage(pack);
            //}
            //finally
            //{
            //    reader.Advance(totalSize);
            //}

            return null;
        }
    }
}
