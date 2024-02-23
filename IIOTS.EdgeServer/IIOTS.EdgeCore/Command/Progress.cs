using IIOTS.EdgeCore.Manage;
using IIOTS.Models;
using IIOTS.Util;
using NetMQ.Sockets; 

namespace IIOTS.EdgeCore.Command
{
    internal static class Progress
    {
        /// <summary>
        /// 类名
        /// </summary>
        private readonly static string className = typeof(Progress).Name;

        /// <summary>
        /// 下发配置信息
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="progressLogin"></param>
        public static void SetConfig(this PublisherSocket publisher, ProgressLoginInfo progressLogin, ProgressManage progressManage)
        {
            //本地模式下发送文件配置
            if (AppConfigurationHelper.Configuration.GetSection("LocalMode").Get<bool>())
            {
                //获取配置路径
                string[] enableConfigPaths = Directory.GetFiles(Config.EnableConfigPath);
                foreach (var enableConfigPath in enableConfigPaths)
                {
                    //读取配置
                    ProgressConfig? progressConfig = File.ReadAllText(enableConfigPath).ToObject<ProgressConfig>();
                    if (progressConfig != null && progressConfig.Name == progressLogin.Name)
                    {
                        foreach (var equConfig in progressConfig.EquConfigs)
                        {
                            if (equConfig.Tags.Count == 0 && equConfig.TagConfigPath != null)
                            {
                                var tags = File.ReadAllText(Path.Combine(Config.TagConfigPath, $"{equConfig.TagConfigPath}.json")).ToObject<List<TagConfig>>();
                                if (tags != null)
                                {
                                    equConfig.Tags = tags ;
                                }
                            }
                        }
                        publisher.PubQueue($"{progressLogin.ClientId}/{className}/SetConfig", progressConfig);
                        return;
                    }
                }
            }
            else
            {
                var progressLogins = progressManage.GetAllProgress().FindAll(p => p.Name == progressLogin.Name);
                foreach (var _progressLogin in progressLogins)
                {
                    publisher.PubQueue($"{_progressLogin.ClientId}/{className}/SetConfig", _progressLogin.progressConfig);
                } 
            }
        }


        /// <summary>
        /// 添加设备
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="clientId"></param>
        /// <param name="equConfig"></param>
        /// <returns></returns>
        public static bool AddEqu(this PublisherSocket publisher, string clientId, EquConfig equConfig)
        {
            return publisher.Reply<bool>($"{clientId}/{className}/AddEqu", equConfig);
        }
        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="clientId"></param>
        /// <param name="eques"></param>
        /// <returns></returns>
        public static bool RemoveEqu(this PublisherSocket publisher, string clientId, List<string> eques)
        {
            return publisher.Reply<bool>($"{clientId}/{className}/RemoveEqu", eques);
        }
        /// <summary>
        /// 启动设备
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="clientId"></param>
        /// <param name="eques"></param>
        /// <returns></returns>
        public static bool StartEqu(this PublisherSocket publisher, string clientId, List<string> eques)
        {
            return publisher.Reply<bool>($"{clientId}/{className}/StartEqu", eques);
        }
        /// <summary>
        /// 停止设备
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="clientId"></param>
        /// <param name="eques"></param>
        /// <returns></returns>
        public static bool StopEqu(this PublisherSocket publisher, string clientId, List<string> eques)
        {
            return publisher.Reply<bool>($"{clientId}/{className}/StopEqu", eques);
        }
    }
}
