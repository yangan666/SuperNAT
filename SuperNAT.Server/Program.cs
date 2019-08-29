using SuperNAT.Common;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

namespace SuperNAT.Server
{
    class Program
    {
        static void Main(string[] args)
        {
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
