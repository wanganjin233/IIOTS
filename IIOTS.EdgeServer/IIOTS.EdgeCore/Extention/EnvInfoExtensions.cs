using IIOTS.EdgeCore.Manage;
using IIOTS.Models;
using IIOTS.Util;

namespace IIOTS.EdgeCore.Extension
{
    public static class EnvInfoExtensions
    {
        /// <summary>
        /// 注入配置
        /// </summary>
        /// <param name="hostBuilder">建造者</param>
        /// <returns></returns>
        public static IHostApplicationBuilder EnvInfo(this IHostApplicationBuilder hostBuilder, string[] args)
        { 
            EdgeLoginInfo edgeLoginInfo = new();
            var edgeID = Environment.GetEnvironmentVariable("EdgeID", EnvironmentVariableTarget.Process);

            if (args.Length > 0)
            {
                edgeLoginInfo.EdgeID = args[0];
            }
            else if (edgeID != null)
            {
                edgeLoginInfo.EdgeID = edgeID;
            }
            else
            {
                edgeLoginInfo.EdgeID = hostBuilder.Configuration.GetSection("EdgeID").Get<string>();
            }
            //获取MQTT 配置
            string? MQTTIP = Environment.GetEnvironmentVariable("MQTTIP", EnvironmentVariableTarget.Process);
            string? MQTTPort = Environment.GetEnvironmentVariable("MQTTPort", EnvironmentVariableTarget.Process);
            MQTTConcat concat = new();
            if (args.Length > 3)
            {
                concat.IP = args[1];
                concat.Port = args[2].ToInt();
            }
            else if (MQTTIP != null && MQTTPort != null)
            {
                concat.IP = MQTTIP;
                concat.Port = MQTTPort.ToInt();
            }
            else
            {
                concat.IP = hostBuilder.Configuration.GetSection("MQTT:IP").Get<string>();
                concat.Port = hostBuilder.Configuration.GetSection("MQTT:Port").Get<int>();
            }
            hostBuilder.Services.AddSingleton(concat);
            hostBuilder.Services.AddSingleton(new ProgressManage(edgeLoginInfo));
            return hostBuilder;
        }
    }
}
