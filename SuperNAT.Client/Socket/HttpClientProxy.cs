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
                                using HttpRequestMessage httpRequest = new HttpRequestMessage()
                                {
                                    Method = new HttpMethod(httpModel.Method),
                                    RequestUri = new Uri($"{map.protocol}://{map.local_endpoint}{httpModel.Route}")
                                };
                                //foreach (var item in httpModel.Headers)
                                //{
                                //    httpRequest.Headers.Add(item.Key, item.Value);
                                //}
                                if (httpRequest.Method != HttpMethod.Get && httpModel.Content?.Length > 0)
                                {
                                    var body = DataHelper.Decompress(httpModel.Content);//解压
                                    httpRequest.Content = new StringContent(body.ToASCII(), Encoding.UTF8, httpModel.ContentType.Split(";")[0]);
                                }
                                var response = await _httpClient.SendAsync(httpRequest);
                                HandleLog.WriteLine($"请求：{httpRequest.RequestUri}，收到返回结果！");
                                //回传给服务器
                                httpModel.Local = map.local_endpoint;
                                httpModel.Headers = response.Headers.ToDictionary();
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
