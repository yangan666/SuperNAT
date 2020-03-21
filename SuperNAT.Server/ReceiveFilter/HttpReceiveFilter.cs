using SuperNAT.AsyncSocket;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using SuperNAT.Common;
using SuperNAT.Model;

namespace SuperNAT.Server
{
    public class HttpReceiveFilter : IReceiveFilter<HttpRequestInfo>
    {
        //private List<byte> raw = new List<byte>();
        private HttpRequestInfo _httpRequestInfo = null;
        private static object lockObj = new object();
        public HttpRequestInfo Filter(ref SequenceReader<byte> reader)
        {
            lock (lockObj)
            {
                try
                {
                    if (_httpRequestInfo == null || (_httpRequestInfo != null && _httpRequestInfo.FilterStatus == FilterStatus.Completed))
                        _httpRequestInfo = new HttpRequestInfo();

                    //raw.AddRange(reader.Sequence.ToArray());

                    LoadRequestLine(ref reader);
                    LoadRequestHeader(ref reader);
                    LoadRequestBody(ref reader);

                    if (_httpRequestInfo.FilterStatus != FilterStatus.Completed)
                        return null;

                    //_httpRequestInfo.Raw = raw.ToArray();
                    _httpRequestInfo.BaseUrl = _httpRequestInfo.Headers["Host"];
                    _httpRequestInfo.Success = true;
                    _httpRequestInfo.Message = "HTTP请求解析成功";
                    return _httpRequestInfo;
                }
                catch (Exception ex)
                {
                    HandleLog.WriteLine($"http解析异常：{ex}");
                    _httpRequestInfo.FilterStatus = FilterStatus.Completed;
                    return new HttpRequestInfo() { Success = false, Message = ex.Message };
                }
            }
        }

        public virtual HttpModel DecodePackage(ref HttpModel httpModel)
        {
            httpModel.HttpVersion = _httpRequestInfo.HttpVersion;
            httpModel.Method = _httpRequestInfo.Method;
            httpModel.Path = _httpRequestInfo.Path;
            httpModel.Headers = _httpRequestInfo.Headers;
            httpModel.Host = _httpRequestInfo.BaseUrl;
            httpModel.ContentType = _httpRequestInfo.ContentType;
            httpModel.Content = _httpRequestInfo.Body;

            return httpModel;
        }

        public void Reset()
        {
            //raw.Clear();
            _httpRequestInfo = null;
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
                    _httpRequestInfo.HttpVersion = subItems[2];
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
                    var value = line.SubLeftWith(':', out string name);
                    if (name == "Content-Length")
                    {
                        _httpRequestInfo.ContentLength = int.Parse(value);
                    }
                    if (name == "Content-Type")
                    {
                        _httpRequestInfo.ContentType = value;
                    }
                    _httpRequestInfo.Headers.Add(name, value);
                }
            }
        }

        private void LoadRequestBody(ref SequenceReader<byte> reader)
        {
            if (_httpRequestInfo.FilterStatus == FilterStatus.LoadingBody)
            {
                var position = reader.Consumed;
                var rest = reader.Length - position;
                if (rest >= _httpRequestInfo.ContentLength)
                {
                    _httpRequestInfo.Body = reader.Sequence.Slice(position, _httpRequestInfo.ContentLength).ToArray();
                    reader.Advance(rest);//多余的不要了
                    _httpRequestInfo.FilterStatus = FilterStatus.Completed;
                }
            }
        }

        private static bool TryReadLine(ref SequenceReader<byte> reader, out string line)
        {
            // Look for a \n in the buffer.
            var readLine = reader.TryReadTo(out ReadOnlySequence<byte> lineBytes, (byte)'\n', true);

            if (!readLine)
            {
                line = default;
                return false;
            }

            StringBuilder lineStr = new StringBuilder();
            foreach (var segment in lineBytes)
            {
                lineStr.Append(Encoding.UTF8.GetString(segment.Span));
            }
            line = lineStr.ToString();
            // Skip the line + the \n.
            line = line[0..^1];

            return true;
        }
    }
}
