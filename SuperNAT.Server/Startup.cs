using CSuperSocket.SocketBase.Config;
using Dynamic.Core.Log;
using Dynamic.Core.Service;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using SuperNAT.Server.Extions;

namespace SuperNAT.Server
{
    public class Startup
    {
        public static void Init()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", false);
            var configuration = builder.Build();

            var appSettingSetion = configuration.GetSection("AppSettingConfig");
            var appSettingConfig = appSettingSetion.Get<AppSettingConfig>();
            IocUnity.AddSingleton<AppSettingConfig>(appSettingConfig);

            //var setion = configuration.GetSection("SimpleSocketConfig");
            //var simpleConfig = setion.Get<SimpleSocketConfig>();
            //IocUnity.AddSingleton<SimpleSocketConfig>(simpleConfig);

            var loggerSection = configuration.GetSection("LogConfig");
            var logConfig = loggerSection.Get<LogConfig>();
            logConfig.LogBaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logConfig.LogBaseDir);
            IocUnity.AddSingleton<LogConfig>(logConfig);

            InitAppSetting();
            InitLog();

        }

        public static void InitAppSetting()
        {
            AppSettingConfig appConfig = IocUnity.Get<AppSettingConfig>();
            appConfig.LoadAppConfig();

        }

        public static void InitLog()
        {
            LogConfig logConfig = IocUnity.Get<LogConfig>();
            LoggerManager.InitLogger(logConfig);

            LoggerManager.GetLogger("LogInit").Error("Test日志组件初始化成功,当前系统运行平台:{0}", RuntimeInformation.OSDescription);

            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            LoggerManager.GetLogger("GlobUnhandledException").Error(ex.ToString());

            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = oldColor;
        }
    }
}
