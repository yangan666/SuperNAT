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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;

namespace SuperNAT.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(option =>
            {
                option.EnableEndpointRouting = false;
            })
            .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //app.UseRouting(routes =>
            //{
            //    routes.MapControllers();
            //});

            app.UseAuthorization();

            app.UseCors(builder =>
            {
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
                builder.AllowAnyOrigin();
                //builder.AllowCredentials();
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Show}/{action=Index}/{id?}");
            });
            DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
            defaultFilesOptions.DefaultFileNames.Clear();
            defaultFilesOptions.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(defaultFilesOptions);
            app.UseStaticFiles(new StaticFileOptions
            {
                //FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory()),
                //设置不限制content-type 该设置可以下载所有类型的文件，但是不建议这么设置，因为不安全
                //ServeUnknownFileTypes = true 
                //下面设置可以下载apk和nupkg类型的文件
                ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
                    {
                        { ".apk","application/vnd.android.package-archive"},
                        { ".nupkg","application/zip"}
                    })
            });
            app.UseStaticFiles();
        }



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

    public class DefaultContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName;
        }
    }
}
