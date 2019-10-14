using CSuperSocket.Common;
using CSuperSocket.Facility.Protocol;
using CSuperSocket.SocketBase.Protocol;
using SuperNAT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Server
{
    public class HttpReceiveFilter : IReceiveFilter<HttpRequestInfo>
    {
        public int LeftBufferSize { get; set; }

        public IReceiveFilter<HttpRequestInfo> NextReceiveFilter { get; set; }

        public FilterState State { get; set; }
        public static byte[] EndHeader = Encoding.UTF8.GetBytes("\r\n\r\n");

        /// <summary>
        /// 实现http请求报文解析
        /// </summary>
        /// <param name="readBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="toBeCopied"></param>
        /// <param name="rest"></param>
        /// <returns></returns>
        public HttpRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            rest = 0;
            var bodyBytes = new byte[length];
            Array.Copy(readBuffer, offset, bodyBytes, 0, length);

            //包含结束符截断报文解析
            var headerIndex = DataHelper.BytesIndexOf(bodyBytes, Encoding.UTF8.GetBytes("\r\n\r\n"));
            if (headerIndex == -1)
            {
                //头部未结束 Tips：当你在接收缓冲区中没有找到一个完整的请求时, 你需要返回 NULL.
                return null;
            }

            //判断是否存在Content-Length
            var result = Encoding.UTF8.GetString(bodyBytes.CloneRange(0, headerIndex));
            if (!result.Contains("Content-Length"))
            {
                //不包含Body直接返回 Tips：已经是完整报文
                return new HttpRequestInfo(result, result);
            }

            //解析头部找到Content-Length
            var firstLine = string.Empty;
            var headers = new Dictionary<string, string>();
            string[] headerStr = result.Substring(0, headerIndex).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var lenStr = headerStr.FirstOrDefault(c => c.Contains("Content-Length"));
            var lenArr = lenStr.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
            //Body长度
            var contentLen = Convert.ToInt32(lenStr[1]);
            //当前接收的Body长度
            var receiveLen = length - headerIndex - 4;
            if (receiveLen < contentLen)
            {
                //Body未结束 Tips：当你在接收缓冲区中没有找到一个完整的请求时, 你需要返回 NULL.
                return null;
            }
            else if (receiveLen > contentLen)
            {
                //当你在接收缓冲区中找到一条完整的请求, 但接收到的数据并不仅仅包含一个请求时，设置剩余数据的长度到输出变量 "rest".
                rest = receiveLen - contentLen;
                var @return = bodyBytes.CloneRange(0, length - rest);
            }

            //返回一个完整的http请求实例
            return new HttpRequestInfo(result, Encoding.UTF8.GetString(bodyBytes.CloneRange(headerIndex + 4, length - headerIndex - 4)));
        }

        public void Reset()
        {

        }
    }
}
