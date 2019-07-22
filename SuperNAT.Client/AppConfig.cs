using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNAT.Client
{
    public class AppConfig
    {
        public static string GetSetting(string key)
        {
            var build = new ConfigurationBuilder();
            build.SetBasePath(Directory.GetCurrentDirectory());
            build.AddJsonFile("//appsettings.json", true, true);
            var dbConfig = build.Build();
            var val = dbConfig.GetSection(key).Value;

            return val;
        }
    }
}
