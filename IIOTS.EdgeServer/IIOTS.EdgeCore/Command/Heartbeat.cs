using NetMQ.Sockets;
using IIOTS.Util;

namespace IIOTS.EdgeCore.Command
{
    internal static class Heartbeat
    {
        /// <summary>
        /// 类名
        /// </summary>
        private readonly static string className = typeof(Heartbeat).Name;
        /// <summary>
        /// 检测是否活跃
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="progressLogin"></param>
        /// <returns></returns>
        public static long CheckActive(this PublisherSocket publisher, string clientId)
        {
            return publisher.Reply<long>($"{clientId}/{className}/CheckActive", clientId);
        }
        /// <summary>
        /// 确认活跃
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="progressLogin"></param>
        /// <returns></returns>
        public static void ConfirmActive(this PublisherSocket publisher, string clientId, long workerId)
        {
            publisher.Send($"{clientId}/{className}/ConfirmActive", workerId);
        }
    }
}
