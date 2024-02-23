using IIOTS.Interface;
using IIOTS.Util; 

namespace IIOTS.EdgeDriver.Handler
{
    public class HeartbeatHandler(ILoggerFactory loggerFactory, AutoResetEvent autoResetEvent) : IHandler
    {
        private readonly ILogger<HeartbeatHandler> logger = loggerFactory.CreateLogger<HeartbeatHandler>();
        /// <summary>
        /// 响应活跃检测
        /// </summary>
        /// <returns></returns>
        public long CheckActive(string ClientId)
        {
            logger.LogTrace($"接收到心跳请求{ClientId}");
            return IdHelper.WorkerId;
        }
        /// <summary>
        /// 确认活跃
        /// </summary>
        /// <param name="workerId"></param>
        public void ConfirmActive(long workerId)
        {
            if (IdHelper.WorkerId == workerId)
            {
                logger.LogTrace($"确认{Config.Identifier}活跃");
                autoResetEvent.Set();
            }
        }
    }
}
