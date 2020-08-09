using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Core
{
    public class PackHelper
    {
        /// <summary>
        /// 报文封装：帧头（01）+数据长度（7）+正文数据（json字符串）+帧尾（04）
        /// </summary>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public static byte[] CreatePack(JsonData jsonData)
        {
            var json = jsonData.ToJson();
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            var sendBytes = new List<byte>() { 0x1 };
            //长度为ASCII字符串，比如长度为100，则字符串为0000100，16进制为30303030313030
            var lenBytes = Encoding.UTF8.GetBytes(jsonBytes.Length.ToString().PadLeft(7, '0'));
            sendBytes.AddRange(lenBytes);
            sendBytes.AddRange(jsonBytes);
            sendBytes.Add(0x04);

            return sendBytes.ToArray();
        }
    }
}
