using CSuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Server
{
    public class HttpRequestInfo : IRequestInfo
    {
        public HttpRequestInfo(string[] header, byte[] content, byte[] data)
        {
            Header = header;
            Content = content;
            Data = data;
        }

        public string Key { get; set; }
        public string[] Header { get; set; }
        public byte[] Content { get; set; }
        public byte[] Data { get; set; }
        //public string DataString => Data == null ? "" : Encoding.UTF8.GetString(Data);//影响性能

        public string RequestInfo { get; set; }
        public string Method { get; set; }
        public string Route { get; set; }
        public Dictionary<string, string> HeaderDict
        {
            get
            {
                var headers = new Dictionary<string, string>();

                try
                {
                    for (int i = 0; i < Header.Length; i++)
                    {
                        string[] temp = Header[i].Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
                        if (temp.Length == 2)
                        {
                            if (headers.ContainsKey(temp[0]))
                            {
                                headers[temp[0]] = temp[1];
                            }
                            else
                            {
                                headers.Add(temp[0], temp[1]);
                            }
                        }
                        else
                        {
                            RequestInfo = string.Join("", temp);
                            var headItems = RequestInfo.Split(' ');
                            Method = headItems[0];
                            Route = headItems[1].Trim();
                        }
                    }
                }
                catch (Exception ex)
                {

                }

                return headers;
            }
        }
    }
}
