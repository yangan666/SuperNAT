using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SuperNAT.Server.Models;
using SuperNAT.Server.Auth;
using Microsoft.AspNetCore.Mvc;
using log4net.Repository;
using log4net;
using log4net.Config;
using SuperNAT.Common;
using SuperNAT.Bll;
using System.Threading.Tasks;
using System.Threading;
using SuperNAT.AsyncSocket;
using Microsoft.Extensions.Logging;

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
            services.AddLogging(logging =>
           {
               logging.AddConsole();
               logging.AddDebug();
           });

            services.AddHttpContextAccessor();
            services.AddScoped<IIdentityService, IdentityService>();

            var jwtSettingConfiguration = Configuration.GetSection("JwtSetting");
            var jwtSetting = jwtSettingConfiguration.Get<JwtSetting>();
            services.Configure<JwtSetting>(jwtSettingConfiguration);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtSetting.Issuer,
                    ValidAudience = jwtSetting.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.SecurityKey))
                };
            });

            services.AddMvc(option =>
            {
                option.EnableEndpointRouting = false;
            })
            .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);
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

            app.UseCors(builder =>
            {
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
                builder.AllowAnyOrigin();
                //builder.AllowCredentials();
            });

            app.UseAuthorization();
            app.UseMiddleware<AuthMiddleware>();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Show}/{action=Index}/{id?}");
            });
            app.UseMvc();

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

        static object lockLog = new object();//日志锁
        public static void Init()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", false);
            var configuration = builder.Build();

            //加载配置到全局
            GlobalConfig.ConnetionString = configuration.GetValue<string>("DBConfig:ConnetionString");
            GlobalConfig.NatPort = configuration.GetValue<int>("ServerConfig:NatPort");
            GlobalConfig.ServerPort = configuration.GetValue<int>("ServerConfig:ServerPort");
            GlobalConfig.DefaultUrl = configuration.GetValue<string>("ServerConfig:DefaultUrl");
            GlobalConfig.RegRoleId = configuration.GetValue<string>("ServerConfig:RegRoleId");

            var repository = LogManager.CreateRepository("NETCoreRepository");
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
            Log4netUtil.LogRepository = repository;//类库中定义的静态变量
            LogHelper.WriteLog += (level, log, isPrint) =>
            {
                lock (lockLog)
                {
                    switch (level)
                    {
                        case LogLevel.Debug:
                            Log4netUtil.Debug(log);
                            break;
                        case LogLevel.Information:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Log4netUtil.Info(log);
                            break;
                        case LogLevel.Warning:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Log4netUtil.Warn(log);
                            break;
                        case LogLevel.Error:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Log4netUtil.Error(log);
                            break;
                        case LogLevel.Critical:
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Log4netUtil.Fatal(log);
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Log4netUtil.Info(log);
                            break;
                    }
                    if (isPrint)
                    {
                        Console.Write(LogHelper.GetString(level));
                        Console.ResetColor();
                        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss,ffff} {log}");
                    }
                }
            };

            Task.Run(() =>
            {
                while (true)
                {
                    //更新假在线的主机
                    var bll = new ClientBll();
                    var res = bll.UpdateOfflineClient();
                    LogHelper.Info(res.Message, false);
                    Thread.Sleep(60000);
                }
            });

            var appSettingSetion = configuration.GetSection("AppSettingConfig");
            var appSettingConfig = appSettingSetion.Get<AppSettingConfig>();

            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            LogHelper.Info(ex.Message);
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
