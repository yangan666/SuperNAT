using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Client
{
    public class HttpClientProxy
    {
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
                                using HttpRequestMessage httpRequest = new HttpRequestMessage()
                                {
                                    Method = new HttpMethod(httpModel.Method),
                                    RequestUri = new Uri($"{map.protocol}://{map.local_endpoint}{httpModel.Path}")
                                };
                                if (httpRequest.Method != HttpMethod.Get && httpModel.Content?.Length > 0)
                                {
                                    var body = DataHelper.Decompress(httpModel.Content);//解压
                                    httpRequest.Content = new StringContent(body.ToUTF8String(), Encoding.UTF8, httpModel.ContentType.Split(";")[0]);
                                }
                                using HttpClient _httpClient = new HttpClient();
                                foreach (var item in httpModel.Headers)
                                {
                                    if (item.Key != "Content-Type")
                                    {
                                        if (!httpRequest.Content?.Headers.TryAddWithoutValidation(item.Key, item.Value) ?? true)
                                        {
                                            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);
                                        }
                                    }
                                }
                                var response = await _httpClient.SendAsync(httpRequest);
                                //回传给服务器
                                httpModel.HttpVersion = response.Version.ToString();
                                httpModel.StatusCode = (int)response.StatusCode;
                                httpModel.StatusMessage = response.StatusCode.ToString();
                                httpModel.Local = map.local_endpoint;
                                httpModel.Headers = response.Headers.ToDictionary();
                                httpModel.ResponseTime = DateTime.Now;
                                foreach (var item in response.Content.Headers)
                                {
                                    httpModel.ContentHeaders.Add(item.Key, string.Join(";", item.Value));
                                    if (item.Key == "Content-Type")
                                    {
                                        httpModel.ContentType = string.Join(";", item.Value);
                                    }
                                }
                                var returnContent = DataHelper.StreamToBytes(response.Content.ReadAsStreamAsync().Result);
                                httpModel.Content = DataHelper.Compress(returnContent);
                                var pack = PackHelper.CreatePack(new JsonData()
                                {
                                    Type = (int)JsonType.HTTP,
                                    Action = (int)HttpAction.Response,
                                    Data = httpModel.ToJson()
                                });
                                natClient?.Send(pack);
                                HandleLog.WriteLine($"{map.name} {httpModel.Method} {httpRequest.RequestUri.AbsoluteUri} {httpModel.StatusCode} {httpModel.StatusMessage}");
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    HandleLog.WriteLine($"处理请求异常：{ex}");
                }
            });
        }
    }
}
