using System;
using System.Collections.Generic;
using System.IO;
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
            HttpResponseMessage result = new HttpResponseMessage();
            HttpClient client = null;
            try
            {
                if (url.StartsWith("https"))
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
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
                        case "OPTIONS":
                            result.StatusCode = HttpStatusCode.NoContent;
                            result.Content = new StringContent("");
                            break;
                    }
                }

                Log4netUtil.Info($"请求地址：{url}{Environment.NewLine}请求参数：{postData}{Environment.NewLine}返回结果：{result.ToString()}");
            }
            catch (Exception ex)
            {
                Log4netUtil.Error("Api接口出错了", ex.InnerException ?? ex);
                Console.WriteLine($"Api接口出错了：{ex.InnerException ?? ex}");
            }
            finally
            {
                client.Dispose();
            }

            return result;
        }

        public static string HttpRequest(string method, string url, string postData = null, Dictionary<string, string> headers = null, string contentType = null, int timeout = 60, Encoding encoding = null)
        {
            string result = string.Empty;
            HttpResponseMessage response = null;
            HttpClient client = null;
            try
            {
                if (url.StartsWith("https"))
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
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
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
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
                            response = client.PostAsync(url, content).Result;
                            break;
                        case "GET":
                            response = client.GetAsync(url).Result;
                            break;
                        case "PUT":
                            response = client.PutAsync(url, content).Result;
                            break;
                        case "DELETE":
                            response = client.DeleteAsync(url).Result;
                            break;
                    }
                }
                using (response)
                {
                    result = response.Content.ReadAsStringAsync().Result;
                    Log4netUtil.Info($"请求地址：{url}{Environment.NewLine}请求参数：{postData}{Environment.NewLine}返回结果：{result}");
                }
            }
            catch (Exception ex)
            {
                Log4netUtil.Error("Api接口出错了", ex.InnerException ?? ex);
                Console.WriteLine($"Api接口出错了：{ex.InnerException ?? ex}");
            }
            finally
            {
                client.Dispose();
            }

            return result;
        }

        public static string HttpsRequest(string method, string url, string postData = null, Dictionary<string, string> headers = null, string contentType = null, int timeout = 60, Encoding encoding = null)
        {
            string result = string.Empty;

            try
            {
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
                X509Certificate2 cerCaiShang = new X509Certificate2(GlobalConfig.CertFile, GlobalConfig.CertPassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
                httpRequest.ClientCertificates.Add(cerCaiShang);

                httpRequest.Method = method;
                if (!string.IsNullOrEmpty(contentType))
                {
                    httpRequest.ContentType = contentType;
                }
                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        httpRequest.Headers.Add(header.Key, header.Value);
                    }
                }
                if (method != "Get" && !string.IsNullOrEmpty(postData))
                {
                    using Stream requestStem = httpRequest.GetRequestStream();
                    using StreamWriter sw = new StreamWriter(requestStem);
                    sw.Write(postData);
                }

                HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                Stream receiveStream = httpResponse.GetResponseStream();
                using StreamReader sr = new StreamReader(receiveStream);
                result = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                Log4netUtil.Error("Api接口出错了", ex.InnerException ?? ex);
                Console.WriteLine($"Api接口出错了：{ex.InnerException ?? ex}");
            }

            return result;
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
