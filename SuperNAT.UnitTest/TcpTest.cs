using Microsoft.VisualStudio.TestTools.UnitTesting;
using SuperNAT.AsyncSocket;
using SuperNAT.Common;
using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SuperNAT.UnitTest
{
    [TestClass]
    public class TcpTest
    {
        [TestMethod]
        public void TestTcp()
        {
            HandleLog.WriteLog = (s, p) =>
            {
                Console.WriteLine(s);
            };
            var hex = "7E7E010040803540000037002B02FCA7170206163612F1F1004080354050F0F01702061636201900000026190131433923009980993812118203BAFE";
            var bytes = DataHelper.HexToByte(hex);

            SocketClient<RequestInfo> socketClient = new SocketClient<RequestInfo>(new ClientOptions()
            {
                Ip = "139.155.104.69",
                Port = 10007,
                NoDelay = true,
                ProtocolType = ProtocolType.Tcp
            });
            socketClient.OnConnected += (s) =>
            {

            };
            socketClient.OnReceived += (s, r) =>
            {
                HandleLog.WriteLine($"【{socketClient.Local}】收到数据:{r.Raw.ToHexWithSpace()}");
            };
            socketClient.OnClosed += (s) =>
            {
                HandleLog.WriteLine($"【{socketClient.Local}】断开连接");
            };

            Task.Run(async () =>
            {
                await socketClient.ConnectAsync();

                Assert.IsTrue(socketClient.IsConnected);
            });

            while (true)
            {
                Thread.Sleep(500);
                if (socketClient.IsConnected)
                    socketClient.Send(bytes);
            }
        }
    }
}
