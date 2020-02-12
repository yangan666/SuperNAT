using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Common
{
    public static class ByteExtensions
    {
        public static byte[] CloneRange(this byte[] source, int offset, int len)
        {
            var data = new byte[len];
            Array.Copy(source, offset, data, 0, len);
            return data;
        }

        public static string ToHex(this byte[] source)
        {
            return ToHexWithSpace(source).Replace(" ", "");
        }

        public static string ToHexWithSpace(this byte[] source)
        {
            if (source == null)
                return string.Empty;
            return DataHelper.ByteToHex(source);
        }

        public static string ToASCII(this byte[] source)
        {
            if (source == null)
                return string.Empty;
            return Encoding.UTF8.GetString(source);
        }
    }
}
