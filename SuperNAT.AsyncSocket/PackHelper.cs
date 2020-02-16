using SuperNAT.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.AsyncSocket
{
    public class PackHelper
    {
        public static byte[] CreatePack(JsonData jsonData)
        {
            var json = jsonData.ToJson();
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            var sendBytes = new List<byte>() { 0x1 };
            var lenBytes = Encoding.UTF8.GetBytes(jsonBytes.Length.ToString().PadLeft(7, '0'));
            sendBytes.AddRange(lenBytes);
            sendBytes.AddRange(jsonBytes);
            sendBytes.Add(0x04);

            return sendBytes.ToArray();
        }
    }
}
