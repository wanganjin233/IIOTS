using IIOTS.EdgeCore.Command;
using IIOTS.EdgeCore.Manage;
using IIOTS.Interface;
using IIOTS.Models;
using NetMQ.Sockets;

namespace IIOTS.EdgeCore.Handler
{
    public class EquHandler(ILoggerFactory loggerFactory, PublisherSocket publisher, ProgressManage progressManage) : IHandler
    {
        private readonly ILogger<EquHandler> logger = loggerFactory.CreateLogger<EquHandler>();
        /// <summary>
        /// 配置设备
        /// </summary>
        /// <param name="operate"></param>
        /// <returns></returns>
        public bool DeployEqu(Operate<EquConfig> operate)
        {
            if (operate.Content.Enable)
            {
                //添加到进程管理
                progressManage.AddEquConfig(operate.Id, operate.Content);
                if (progressManage.ProgressRunList(operate.Id).Count > 0)
                {
                    return AddEqu(operate);
                }
            }
            else
            {
                progressManage.RemoveEquConfig(operate.Id, operate.Content);
                if (progressManage.ProgressRunList(operate.Id).Count > 0)
                {
                    return RemoveEqu(new Operate<List<string>>()
                    {
                        Id = operate.Id,
                        Content = [operate.Content.EQU]
                    });
                }
            }
            return true;
        }
        #region 操作设备 
        /// <summary>
        /// 添加设备
        /// </summary>
        /// <param name="operate"></param>
        /// <returns></returns>
        public bool AddEqu(Operate<EquConfig> operate)
        {

            if (publisher.AddEqu(operate.Id, operate.Content))
            {
                logger.LogInformation($"节点【{operate.Id}】新增设备【{operate.Content.EQU}】成功");
                return true;
            }
            else
            {
                logger.LogInformation($"节点【{operate.Id}】新增设备【{operate.Content.EQU}】失败");
                return false;
            }
        }
        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="operate"></param>
        /// <returns></returns>
        public bool RemoveEqu(Operate<List<string>> operate)
        {
            if (publisher.RemoveEqu(operate.Id, operate.Content))
            {
                logger.LogInformation($"节点【{operate.Id}】删除设备【{string.Join("|", operate.Content)}】成功");
                return true;
            }
            else
            {
                logger.LogInformation($"节点【{operate.Id}】删除设备【{string.Join("|", operate.Content)}】失败");
                return false;
            }
        }
        /// <summary>
        /// 启动设备
        /// </summary>
        /// <param name="operate"></param>
        /// <returns></returns>
        public bool StartEqu(Operate<List<string>> operate)
        {
            if (publisher.StartEqu(operate.Id, operate.Content))
            {
                logger.LogInformation($"节点【{operate.Id}】启动设备【{string.Join("|", operate.Content)}】成功");
                return true;
            }
            else
            {
                logger.LogInformation($"节点【{operate.Id}】启动设备【{string.Join("|", operate.Content)}】失败");
                return false;
            }
        }
        /// <summary>
        /// 停止设备
        /// </summary>
        /// <param name="operate"></param>
        /// <returns></returns>
        public bool StopEqu(Operate<List<string>> operate)
        {
            if (publisher.StopEqu(operate.Id, operate.Content))
            {
                logger.LogInformation($"节点【{operate.Id}】停止设备【{string.Join("|", operate.Content)}】成功");
                return true;
            }
            else
            {
                logger.LogInformation($"节点【{operate.Id}】停止设备【{string.Join("|", operate.Content)}】失败");
                return false;
            }
        }
        #endregion 
    }
}
