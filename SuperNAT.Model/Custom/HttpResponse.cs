using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperNAT.Model
{
    public class HttpResponse
    {
        public HttpResponse()
        {

        }

        public string HttpVersion { get; set; } = "HTTP/1.1";

        public int Status { get; set; }

        public string StatusMessage { get; set; } = "OK";

        public string ContentType { get; set; }

        public byte[] Body { get; set; }

        public Dictionary<string, string> Headers = new Dictionary<string, string>();


        public byte[] Write()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{HttpVersion} {Status} {StatusMessage}");
            foreach (var item in Headers)
            {
                sb.AppendLine($"{item.Key}: {item.Value}");
            }
            //关闭连接
            sb.AppendLine("Connection: close");

            if (Body?.Count() > 0)
            {
                sb.AppendLine($"Content-Type: {ContentType}");
                sb.AppendLine($"Content-Length: {Body.Length}");
            }
            sb.AppendLine("");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString()).ToList();
            if (Body?.Count() > 0)
            {
                bytes.AddRange(Body);
            }

            return bytes.ToArray();
        }
    }
}
