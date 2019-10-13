using SuperNAT.Common;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System.Threading;
using System.Reflection;

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
            Dapper.SimpleCRUD.SetDialect(Dapper.SimpleCRUD.Dialect.MySQL);
            Startup.Init();
            ServerHanlder.Start();
            Task.Run(() =>
            {
                ServerHanlder.CreateHostBuilder(args).Build().Run();
            });

            Console.ReadKey();
        }
    }
}
