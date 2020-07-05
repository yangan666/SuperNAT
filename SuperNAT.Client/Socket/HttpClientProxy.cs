using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Client
{
    public class HttpClientProxy
    {
        public static async void ProcessData(NatClient natClient, NatPackageInfo packageInfo)
        {
            try
            {
                switch (packageInfo.Body.Action)
                {
                    case (byte)HttpAction.Request:
                        {
                            var httpModel = packageInfo.Body.Data.FromJson<HttpModel>();
                            var map = natClient.Client.MapList.Find(c => c.remote_endpoint == httpModel.Host || (c.remote == httpModel.Host && c.remote_port == 80));
                            if (map == null)
                            {
                                HandleLog.WriteLine($"映射不存在，外网访问地址：{httpModel.Host}");
                                return;
                            }
                            using HttpRequestMessage httpRequest = new HttpRequestMessage()
                            {
                                Method = new HttpMethod(httpModel.Method),
                                RequestUri = new Uri($"{map.protocol}://{map.local_endpoint}{httpModel.Path}")
                            };
                            HandleLog.WriteLine($"{map.name} {httpModel.Method} {httpRequest.RequestUri.AbsoluteUri} {httpModel.Headers.ToJson()}{Environment.NewLine}");
                            string bodyStr = string.Empty;
                            if (httpRequest.Method != HttpMethod.Get && httpModel.Content?.Length > 0)
                            {
                                var body = DataHelper.Decompress(httpModel.Content);//解压
                                bodyStr = body.ToUTF8String();
                                httpRequest.Content = httpModel.ContentType == null ? new StringContent(bodyStr, Encoding.UTF8) : new StringContent(bodyStr, Encoding.UTF8, httpModel.ContentType.Split(";")[0]);
                            }
                            HandleLog.WriteLine($"{map.name} {httpModel.Method} {httpRequest.RequestUri.AbsoluteUri}{Environment.NewLine}【Header】{httpModel.Headers.ToJson()}{$"{Environment.NewLine}【Body】{bodyStr}".If(httpModel.Content?.Length < 1024)}{Environment.NewLine}");
                            using HttpClient _httpClient = new HttpClient();
                            //替换Host 不然400 Bad Request
                            httpModel.Headers["Host"] = map.local_endpoint;
                            foreach (var item in httpModel.Headers)
                            {
                                if (!item.Key.EqualsWhithNoCase("Content-Type"))
                                {
                                    if (!httpRequest.Content?.Headers.TryAddWithoutValidation(item.Key, item.Value) ?? true)
                                    {
                                        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);
                                    }
                                }
                            }
                            if (map.protocol == "https")
                            {
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                            }
                            var response = await _httpClient.SendAsync(httpRequest);
                            //回传给服务器
                            httpModel.HttpVersion = $"{map.protocol.ToUpper()}/{response.Version.ToString()}";
                            httpModel.StatusCode = (int)response.StatusCode;
                            httpModel.StatusMessage = response.StatusCode.ToString();
                            httpModel.Local = map.local_endpoint;
                            httpModel.Headers = response.Headers.ToDictionary();
                            httpModel.ResponseTime = DateTime.Now;
                            foreach (var item in response.Content.Headers)
                            {
                                if (item.Key.EqualsWhithNoCase("Content-Type"))
                                {
                                    httpModel.ContentType = string.Join(";", item.Value);
                                }
                                else
                                {
                                    if (item.Key.EqualsWhithNoCase("Content-Length"))
                                        continue;
                                    httpModel.Headers.Add(item.Key, string.Join(";", item.Value));
                                }
                            }
                            httpModel.Headers.Remove("Transfer-Encoding");//response收到的是完整的 这个响应头要去掉 不然浏览器解析出错
                            var returnContent = DataHelper.StreamToBytes(response.Content.ReadAsStreamAsync().Result);
                            if (returnContent.Length > 0)
                                httpModel.Content = DataHelper.Compress(returnContent);
                            var pack = PackHelper.CreatePack(new JsonData()
                            {
                                Type = (int)JsonType.HTTP,
                                Action = (int)HttpAction.Response,
                                Data = httpModel.ToJson()
                            });
                            natClient?.Send(pack);
                            HandleLog.WriteLine($"{map.name} {httpModel.Method} {httpRequest.RequestUri.AbsoluteUri}{$"{returnContent.ToUTF8String()}".If(returnContent.Length < 1024)} {httpModel.StatusCode} {httpModel.StatusMessage} {Math.Round(returnContent.Length * 1.00 / 1024, 1)}KB{Environment.NewLine}");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                HandleLog.WriteLine($"处理请求异常：{ex}");

                var pack = PackHelper.CreatePack(new JsonData()
                {
                    Type = (int)JsonType.HTTP,
                    Action = (int)HttpAction.Response,
                    Data = new HttpModel()
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        ContentType = "text/html",
                        Content = DataHelper.Compress(Encoding.UTF8.GetBytes($"server error"))
                    }.ToJson()
                });
                natClient?.Send(pack);
            }
        }
    }
}
