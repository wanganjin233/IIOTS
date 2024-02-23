 using IIOTS.EdgeCore.Manage;
using IIOTS.Interface;
using IIOTS.Models;
using IIOTS.Util; 
using NetMQ.Sockets;

namespace IIOTS.EdgeCore.Handler
{
    public class ConfigHandler(ILoggerFactory loggerFactory, PublisherSocket publisher, ProgressManage progressManage) : IHandler
    {
        private readonly ILogger<EquHandler> logger = loggerFactory.CreateLogger<EquHandler>(); 
        /// <summary>
        /// 初始化配置
        /// </summary>
        /// <param name="operate"></param>
        /// <returns></returns>
       // public void Load(List<ProgressConfig> progressConfigs)
       // {
       //     logger.LogInformation($"加载初始配置");
       //     foreach (var progressConfig in progressConfigs)
       //     {
       //         //去除重复启动进程配置
       //         progressConfig.Operations = progressConfig.Operations
       //         .Where((x, i) => progressConfig.Operations.FindIndex(s => s == x) == i)
       //         .ToList();
       //         foreach (var Operation in progressConfig.Operations)
       //         {
       //             ProgressLoginInfo progressLoginInfo = new()
       //             {
       //                 Name = progressConfig.Name,
       //                 ClientType = Operation,
       //                 progressConfig = progressConfig
       //             };
       //             //添加到进程管理
       //             progressManage.AddDriverLoginInfo(progressLoginInfo);
       //             logger.LogInformation($"添加到进程【{progressLoginInfo.ClientId}】管理");
       //         }
       //     } 
       // }
        
    }
}
