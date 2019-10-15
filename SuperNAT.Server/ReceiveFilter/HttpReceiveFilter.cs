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
    /// <summary>
    /// http过滤器
    /// </summary>
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
            string[] headerArr = null;
            var receiveBytes = new byte[length];
            Array.Copy(readBuffer, offset, receiveBytes, 0, length);
            //var ss = Encoding.UTF8.GetString(receiveBytes);

            //包含结束符截断报文解析
            var headerIndex = DataHelper.BytesIndexOf(receiveBytes, EndHeader);
            if (headerIndex == -1)
            {
                //头部未结束 Tips：当你在接收缓冲区中没有找到一个完整的请求时, 你需要返回 NULL.
                return null;
            }

            //判断是否存在Content-Length
            var headerBytes = receiveBytes.CloneRange(0, headerIndex);
            var headerStr = Encoding.UTF8.GetString(headerBytes);
            if (!headerStr.Contains("Content-Length"))
            {
                //TODO 判断请求头是否有Transfer-Encoding

                //不包含Body直接返回 Tips：已经是完整报文
                headerArr = headerStr.Substring(0, headerIndex).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                return new HttpRequestInfo(headerArr, headerBytes, receiveBytes);
            }

            //解析头部找到Content-Length
            var firstLine = string.Empty;
            var headers = new Dictionary<string, string>();
            headerArr = headerStr.Substring(0, headerIndex).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var lenStr = headerArr.FirstOrDefault(c => c.Contains("Content-Length"));
            var lenArr = lenStr.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
            //Body长度
            var contentLen = Convert.ToInt32(lenArr[1]);
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

                //返回一个完整的http请求实例
                return new HttpRequestInfo(headerArr, receiveBytes.CloneRange(headerIndex + 4, length - rest - headerIndex - 4), receiveBytes);
            }

            //返回一个完整的http请求实例
            return new HttpRequestInfo(headerArr, receiveBytes.CloneRange(headerIndex + 4, length - headerIndex - 4), receiveBytes);
        }

        public void Reset()
        {

        }
    }
}
