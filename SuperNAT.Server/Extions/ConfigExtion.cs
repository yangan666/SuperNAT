using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
namespace SuperNAT.Server.Extions
{
    public static class ConfigExtion
    {
        /// <summary>
        /// 将json的AppSettingConfig节点配置装载到ConfigurationManager.AppSettings当中，如果存在覆盖
        /// </summary>
        /// <param name="appConfig"></param>
        public static void LoadAppConfig(this AppSettingConfig appConfig)
        {
            //获取Configuration对象
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            foreach (var appSet in appConfig.AppSettings)
            {
                var setValue = config.AppSettings.Settings[appSet.Key];
                if (setValue != null)
                {
                    config.AppSettings.Settings.Remove(appSet.Key);
                }
                config.AppSettings.Settings.Add(appSet.Key, appSet.Value);
            }
            //一定要记得保存，写不带参数的config.Save()也可以
            config.Save(ConfigurationSaveMode.Modified);
            //刷新，否则程序读取的还是之前的值（可能已装入内存）
            System.Configuration.ConfigurationManager.RefreshSection("appSettings");

        }
    }
}
