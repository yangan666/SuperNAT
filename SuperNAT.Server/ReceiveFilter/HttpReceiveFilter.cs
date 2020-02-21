using SuperNAT.AsyncSocket;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Server
{
    public class HttpReceiveFilter : IReceiveFilter<HttpRequestInfo>
    {
        private List<byte> raw = new List<byte>();
        private HttpRequestInfo _httpRequestInfo = null;
        public HttpRequestInfo Filter(ref SequenceReader<byte> reader)
        {
            if (_httpRequestInfo == null)
                _httpRequestInfo = new HttpRequestInfo();

            raw.AddRange(reader.Sequence.ToArray());

            LoadRequestLine(ref reader);
            LoadRequestHeader(ref reader);
            LoadRequestBody(ref reader);

            if (_httpRequestInfo.FilterStatus != FilterStatus.Completed)
                return null;

            return _httpRequestInfo;
        }

        public virtual HttpModel DecodePackage()
        {
            HttpModel httpModel = new HttpModel
            {
                HttpVersion = _httpRequestInfo.HttpVersion,
                Method = _httpRequestInfo.Method,
                Path = _httpRequestInfo.Path,
                Headers = _httpRequestInfo.Headers,
                Host = _httpRequestInfo.Headers["Host"],
                Content = _httpRequestInfo.Body
            };
            return httpModel;
        }

        public void Reset()
        {
            _httpRequestInfo = null;
        }

        private static bool TryReadLine(ref SequenceReader<byte> reader, out string line)
        {
            // Look for a EOL in the buffer.
            var readLine = reader.TryReadTo(out ReadOnlySequence<byte> lineBytes, (byte)'\n', true);

            if (!readLine)
            {
                line = default;
                return false;
            }

            // Skip the line + the \n.
            StringBuilder lineStr = new StringBuilder();
            foreach (var segment in lineBytes)
            {
                lineStr.Append(Encoding.UTF8.GetString(segment.Span));
            }
            line = lineStr.ToString();

            return true;
        }

        private void LoadRequestLine(ref SequenceReader<byte> reader)
        {
            if (_httpRequestInfo.FilterStatus == FilterStatus.None)
            {
                if (TryReadLine(ref reader, out string line))
                {
                    var subItems = line.Split(' ');
                    _httpRequestInfo.Method = subItems[0];
                    _httpRequestInfo.Path = subItems[1];
                    _httpRequestInfo.HttpVersion = subItems[3];
                    _httpRequestInfo.FilterStatus = FilterStatus.LoadingHeader;
                }
            }
        }

        private void LoadRequestHeader(ref SequenceReader<byte> reader)
        {
            if (_httpRequestInfo.FilterStatus == FilterStatus.LoadingHeader)
            {
                while (TryReadLine(ref reader, out string line))
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        if (_httpRequestInfo.ContentLength == 0)
                        {
                            _httpRequestInfo.FilterStatus = FilterStatus.Completed;
                        }
                        else
                        {
                            _httpRequestInfo.FilterStatus = FilterStatus.LoadingBody;
                        }
                        return;
                    }
                    var valueArr = line.Split(':');
                    if (valueArr[0] == "Content-Length")
                    {
                        _httpRequestInfo.ContentLength = int.Parse(valueArr[1]);
                    }
                    _httpRequestInfo.Headers.Add(valueArr[0], valueArr[1]);
                }
            }
        }

        private void LoadRequestBody(ref SequenceReader<byte> reader)
        {
            if (_httpRequestInfo.FilterStatus == FilterStatus.LoadingBody)
            {
                if (reader.Length >= _httpRequestInfo.ContentLength)
                {
                    _httpRequestInfo.Body = reader.Sequence.ToArray();
                    _httpRequestInfo.Success = true;
                    _httpRequestInfo.Message = "HTTP请求解析成功";
                    _httpRequestInfo.FilterStatus = FilterStatus.Completed;
                }
            }
        }
    }
}
