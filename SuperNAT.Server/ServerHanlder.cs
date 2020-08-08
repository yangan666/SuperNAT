using SuperNAT.Common;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using SuperNAT.Core;

namespace SuperNAT.Server
{
    public class ServerHanlder
    {
        public void Start(string[] args)
        {
            //初始化配置
            Startup.Init();
            //启动内网穿透服务
            ServerManager.Start();
            //启动WebApi接口服务
            Task.Run(() =>
            {
                CreateHostBuilder(args).Build().Run();
            });
        }

        public void Stop()
        {
            ServerManager.Stop();
        }

        #region SuperNAT管理后台 8088端口
        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                var contentRoot = Directory.GetCurrentDirectory();
                var webRoot = Path.Combine(contentRoot, "wwwroot");
                webBuilder.UseStartup<Startup>()
                        .UseKestrel(options =>
                        {
                            options.Limits.MaxConcurrentConnections = 100;
                            options.Limits.MaxConcurrentUpgradedConnections = 100;
                            options.Limits.MaxRequestBodySize = 104857600;//100M
                            options.Limits.MinRequestBodyDataRate =
                                new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                            options.Limits.MinResponseDataRate =
                                new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                            options.Listen(IPAddress.Any, GlobalConfig.ServerPort);
                            //options.Listen(IPAddress.Any, GlobalConfig.ServerPort, listenOptions =>
                            //{
                            //    listenOptions.UseHttps(GlobalConfig.CertFile, GlobalConfig.CertPassword);
                            //});
                        })
                        .UseContentRoot(contentRoot)  // set content root
                        .UseWebRoot(webRoot);         // set web root
            });
        #endregion
    }
}
