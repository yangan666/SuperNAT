using Microsoft.VisualStudio.TestTools.UnitTesting;
using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperNAT.UnitTest
{
    [TestClass]
    public class HttpTest
    {
        [TestMethod]
        public void TestHttp()
        {
            for (int i = 0; i < 1000; i++)
            {
                using var client = new HttpClient();
                using HttpRequestMessage httpRequest = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://water.supernat.cn:10001/static/js/vendor.c34ca220daf55aa22f65.js"),
                    //Content = new StringContent(new { }.ToJson(), Encoding.UTF8, "application/json")
                };

                var response = client.SendAsync(httpRequest).Result;
                var result = response.Content.ReadAsStringAsync().Result;

                Assert.IsTrue(response.IsSuccessStatusCode && result.Length > 0);
            }
        }
    }
}
