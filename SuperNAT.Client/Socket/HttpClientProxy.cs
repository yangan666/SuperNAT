using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;

namespace SuperNAT.Client
{
    public class HttpClientProxy
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        public static void ProcessData(NatClient natClient, NatPackageInfo packageInfo)
        {
            Task.Run(async () =>
            {
                try
                {
                    switch (packageInfo.Body.Action)
                    {
                        case (byte)HttpAction.Request:
                            {
                                var httpModel = packageInfo.Body.Data.FromJson<HttpModel>();
                                var map = ClientHandler.MapList.Find(c => c.remote_endpoint == httpModel.Host);
                                if (map == null)
                                {
                                    HandleLog.WriteLine($"映射不存在，外网访问地址：{httpModel.Host}");
                                    return;
                                }
                                var body = DataHelper.Decompress(httpModel.Content);
                                HttpRequestMessage httpRequest = new HttpRequestMessage()
                                {
                                    Method = new HttpMethod(httpModel.Method),
                                    RequestUri = new Uri($"http://{map.local}/{httpModel.Route}")
                                };
                                if (httpRequest.Method != HttpMethod.Get)
                                {
                                    httpRequest.Content = new ByteArrayContent(body);
                                }
                                var response = await _httpClient.SendAsync(httpRequest);
                                //回传给服务器
                                httpModel.Local = map.local;
                                httpModel.Headers = NameValueCollection();
                                foreach (var item in response.Headers)
                                {
                                    httpModel.Headers.Add(item.Key, string.Join(';', item.Value));
                                }
                                httpModel.Content = DataHelper.StreamToBytes(response.Content.ReadAsStreamAsync().Result);
                                var req = httpModel.Content.ToASCII();
                                var pack = PackHelper.CreatePack(new JsonData()
                                {
                                    Type = (int)JsonType.HTTP,
                                    Action = (int)HttpAction.Response,
                                    Data = httpModel.ToJson()
                                });
                                natClient?.Send(pack);
                                break;
                            }
                    }



                    //var packJson = JsonHelper.Instance.Deserialize<HttpModel>(e.Package.BodyRaw);
                    //var headers = packJson.Headers;
                    //var contentType = headers.ContainsKey("Content-Type") ? headers["Content-Type"] : null;
                    //var data = packJson.Content == null ? "" : Encoding.UTF8.GetString(packJson.Content);
                    //if (!string.IsNullOrEmpty(contentType))
                    //{
                    //    var index = contentType.IndexOf(";");
                    //    if (index > 0)
                    //    {
                    //        //去掉; charset=utf-8
                    //        contentType = contentType.Substring(0, index);
                    //    }
                    //}
                    //if (map == null)
                    //{
                    //    HandleLog.WriteLine($"映射不存在，外网访问地址：{packJson.Host}");
                    //    return;
                    //}
                    //var res = HttpHelper.Request(packJson.Method, $"{map.protocol}://{map.local_endpoint}{packJson.Route}", data, headers: headers, contentType: contentType);
                    //if (res == null)
                    //{
                    //    HandleLog.WriteLine("服务器返回NULL");
                    //    return;
                    //}
                    //using (res)
                    //{
                    //    using var stream = res.Content.ReadAsStreamAsync().Result;
                    //    var result = DataHelper.StreamToBytes(stream);
                    //    var rawResult = Encoding.UTF8.GetString(result);
                    //    StringBuilder resp = new StringBuilder();
                    //    resp.Append($"{map.protocol.ToUpper()}/{res.Version} {(int)res.StatusCode} {res.StatusCode.ToString()}\r\n");
                    //    foreach (var item in res.Headers)
                    //    {
                    //        if (item.Key != "Transfer-Encoding")
                    //        {
                    //            resp.Append($"{item.Key}: {string.Join(";", item.Value)}\r\n");
                    //        }
                    //    }
                    //    foreach (var item in res.Content.Headers)
                    //    {
                    //        resp.Append($"{item.Key}: {string.Join(";", item.Value)}\r\n");
                    //    }
                    //    if (packJson.Method.ToUpper() == "OPTIONS")
                    //    {
                    //        resp.Append("Access-Control-Allow-Credentials: true\r\n");
                    //        if (packJson.Headers.ContainsKey("Access-Control-Request-Headers"))
                    //        {
                    //            resp.Append($"Access-Control-Allow-Headers: {packJson.Headers["Access-Control-Request-Headers"]}\r\n");
                    //        }
                    //        resp.Append("Access-Control-Allow-Methods: *\r\n");
                    //        resp.Append($"Access-Control-Allow-Origin: {packJson.Headers["Origin"]}\r\n");
                    //    }
                    //    if (!res.Content.Headers.Contains("Content-Length"))
                    //    {
                    //        resp.Append($"Content-Length: {result.Length}\r\n");
                    //    }
                    //    resp.Append($"Date: {DateTime.Now}\r\n");
                    //    resp.Append("Connection:close\r\n\r\n");

                    //    var response = Encoding.UTF8.GetBytes(resp.ToString()).ToList();
                    //    response.AddRange(result);

                    //    //先gzip压缩  再转为16进制字符串
                    //    var body = DataHelper.Compress(response.ToArray());
                    //    var pack = new HttpModel()
                    //    {
                    //        Host = packJson.Host,
                    //        UserId = packJson.UserId,
                    //        Content = body,
                    //        ResponseInfo = $"{map.name} {packJson.Method} {packJson.Route} {(int)res.StatusCode} {res.StatusCode.ToString()}"
                    //    };
                    //    var json = JsonHelper.Instance.Serialize(pack);
                    //    var jsonBytes = Encoding.UTF8.GetBytes(json);
                    //    //02 01 数据长度(4) 正文数据(n)   ---http响应包
                    //    var sendBytes = new List<byte>() { 0x2, 0x1 };
                    //    sendBytes.AddRange(BitConverter.GetBytes(jsonBytes.Length).Reverse());
                    //    sendBytes.AddRange(jsonBytes);
                    //    NatClient.Send(sendBytes.ToArray());
                    //    HandleLog.WriteLine(pack.ResponseInfo);
                    //}
                }
                catch (Exception ex)
                {
                    HandleLog.WriteLine($"处理请求异常：{ex}");
                }
            });
        }

        private static NameValueCollection NameValueCollection()
        {
            throw new NotImplementedException();
        }
    }
}
