using System.Configuration;
using System.Reflection;

namespace IIOTS.Util
{
    public static class ConfigurationHelper
    {   /// <summary>
        /// 根据KeyValue获取配置文件AppSetting的值(Common项目的配置获取)
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public static string GetAppSettingValue(string keyValue)
        {
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(AppDomain.CurrentDomain.DynamicDirectory);
            var val = cfg.AppSettings.Settings[keyValue].Value;
            return val;
        }
        /// <summary>
        /// 更新配置文件节点值
        /// </summary>
        /// <param name="keyVaule"></param>
        /// <returns></returns>
        public static string UpdateAppSettingValue(string keyValue, string setValue)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[keyValue].Value = setValue;
            config.Save(ConfigurationSaveMode.Full);
            var rltValue = GetAppSettingValue(keyValue);
            ConfigurationManager.RefreshSection("appSettings");
            return rltValue;
        }
    }
}
