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
            var hex = "01 30 30 37 37 02 7B 22 74 79 70 65 22 3A 22 67 65 74 54 6F 6B 65 6E 22 2C 22 76 65 72 73 69 6F 6E 22 3A 22 31 2E 30 2E 30 2E 30 22 7D 03 36 36 63 33 39 65 62 62 64 32 61 63 36 30 32 66 33 64 32 34 32 32 36 35 31 65 33 61 66 61 31 66 04";
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
