using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperNAT.Server
{
    public class AppSetting
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class AppSettingConfig
    {
        public IList<AppSetting> AppSettings { get; set; }

        public AppSettingConfig()
        {
            this.AppSettings = new List<AppSetting>();
        }
        public string Get(string key)
        {

            return this.AppSettings.Single(f => f.Key == key).Value;

        }

    }
}
