using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common
{
    public class HttpHelper
    {
        public static List<string> DefaultRequestHeadersKeys { get; set; }
        public static HttpResponseMessage Request(string method, string url, string postData = null, Dictionary<string, string> headers = null, string contentType = null, int timeout = 60, Encoding encoding = null)
        {
            HttpResponseMessage result = null;
            HttpClient client = null;
            try
            {
                if (url.StartsWith("https"))
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                }
                //反向代理手动设置cookies
                client = new HttpClient(new HttpClientHandler() { UseCookies = false });

                if (headers != null)
                {
                    if (DefaultRequestHeadersKeys == null)
                    {
                        DefaultRequestHeadersKeys = client.DefaultRequestHeaders.GetType().GetProperties().Select(s => s.Name).ToList();
                    }
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        try
                        {
                            if (DefaultRequestHeadersKeys.Contains(header.Key))
                            {
                                continue;
                            }
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            //Console.WriteLine($"正确添加的header：{header.Key}");
                        }
                        catch
                        {
                            //Console.WriteLine($"错误添加的header：{header.Key}");
                        }
                    }
                }
                if (timeout > 0)
                {
                    client.Timeout = new TimeSpan(0, 0, timeout);
                }
                using (HttpContent content = new StringContent(postData ?? "", encoding ?? Encoding.UTF8))
                {
                    if (!string.IsNullOrEmpty(contentType))
                    {
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                    }
                    //先不释放
                    switch (method.ToUpper())
                    {
                        case "POST":
                            result = client.PostAsync(url, content).Result;
                            break;
                        case "GET":
                            result = client.GetAsync(url).Result;
                            break;
                        case "PUT":
                            result = client.PutAsync(url, content).Result;
                            break;
                        case "DELETE":
                            result = client.DeleteAsync(url).Result;
                            break;
                    }
                }

                Log4netUtil.Info($"请求地址：{url}{Environment.NewLine}请求参数：{postData}{Environment.NewLine}返回结果：{result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                Log4netUtil.Error("Api接口出错了", ex.InnerException ?? ex);
                Console.WriteLine($"Api接口出错了：{ex.InnerException ?? ex}");
                return null;
            }
            finally
            {
                client.Dispose();
            }
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //绕过验证
            return true;
        }
    }

    public enum HttpClientActionMethod
    {
        Post,
        Get,
        Put,
        Delete
    }
}
