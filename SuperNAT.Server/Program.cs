using SuperNAT.Common;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System.Threading;
using System.Reflection;
using Topshelf;
using SuperNAT.AsyncSocket;
using System.Net;

namespace SuperNAT.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            #region 判断当前程序是否启动，如果已启动则退出，保证只有一个实例启动
            bool blnIsRunning;
            Mutex mutexApp = new Mutex(false, Assembly.GetExecutingAssembly().FullName, out blnIsRunning);
            if (!blnIsRunning)
            {
                Console.WriteLine($"SuperNAT.Server已经运行!");
                Console.ReadKey();
                return;
            }
            #endregion
            HandleLog.WriteLog += (log, isPrint) =>
            {
                if (isPrint)
                {
                    Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss,ffff} {log}");
                }
                //Log4netUtil.Info(log);
            };
            SocketServer socketServer = new SocketServer(1000, 1024);
            socketServer.Init();
            socketServer.NewSessionConnected += (e) =>
           {
               HandleLog.WriteLine($"客户端【{e.Socket.RemoteEndPoint}】连接到服务器");
           };
            socketServer.NewRequestReceived += (e, d) =>
            {
                HandleLog.WriteLine($"客户端【{e.Socket.RemoteEndPoint}】收到数据：{e.HexRead}");
            };
            socketServer.SessionClosed += (e) =>
            {
                HandleLog.WriteLine($"客户端【{e.Socket.RemoteEndPoint}】关闭连接");
            };
            socketServer.Start(new IPEndPoint(IPAddress.Any, 10000));

            //HostFactory.Run(x =>
            //{
            //    x.Service<ServerHanlder>(s =>
            //    {
            //        s.ConstructUsing(name => new ServerHanlder());
            //        s.WhenStarted(tc => tc.Start(args));
            //        s.WhenStopped(tc => tc.Stop());
            //    });
            //    x.RunAsLocalSystem();

            //    x.SetDescription("SuperNATServer");
            //    x.SetDisplayName("SuperNATServer");
            //    x.SetServiceName("SuperNATServer");
            //});



            Console.ReadKey();
        }
    }
}
