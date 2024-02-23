using IIOTS.EdgeCore.Command;
using IIOTS.EdgeCore.Manage;
using IIOTS.Interface;
using IIOTS.Models;
using NetMQ.Sockets;

namespace IIOTS.EdgeCore.Handler
{
    public class ProgressHandler(ILoggerFactory loggerFactory, PublisherSocket publisher, ProgressManage progressManage) : IHandler
    {
        private readonly ILogger<ProgressHandler> logger = loggerFactory.CreateLogger<ProgressHandler>();
        /// <summary>
        /// 进程登录
        /// </summary>
        /// <param name="progressLogin"></param>
        public void Login(ProgressLoginInfo progressLogin)
        {
            logger.LogInformation($"接收到进程【{progressLogin.ClientId}】请求登录 ");
            //更新登录信息
            progressManage.UpdateDriverLoginInfo(progressLogin);
            //下发配置信息
            logger.LogInformation($"下发配置信息【{progressLogin.ClientId}】 ");
            publisher.SetConfig(progressLogin,progressManage);
        }
        #region 操作进程 
        /// <summary>
        /// 添加进程
        /// </summary>
        /// <param name="progressLoginInfo"></param>
        /// <returns></returns>
        public bool AddProgress(ProgressLoginInfo progressLoginInfo)
        {
            if (progressManage.AddDriverLoginInfo(progressLoginInfo))
            { 
                logger.LogInformation($"新增节点进程【{progressLoginInfo.ClientId}】成功");
                return true;
            }
            else
            {
                logger.LogInformation($"新增节点进程【{progressLoginInfo.ClientId}】失败");
                return false;
            }
        }
        /// <summary>
        /// 删除进程
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public bool RemoveProgress(string clientId)
        {
            if (progressManage.RemoveDriverLoginInfo(clientId))
            {
                logger.LogInformation($"删除节点进程【{clientId}】成功");
                return true;
            }
            else
            {
                logger.LogInformation($"删除节点进程【{clientId}】失败");
                return false;
            }
        }
        #endregion
    }
}
