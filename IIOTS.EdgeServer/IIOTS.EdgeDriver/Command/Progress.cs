using IIOTS.Models;
using NetMQ.Sockets;
using IIOTS.Util;

namespace IIOTS.EdgeDriver.Command
{
    internal static class Progress
    {
        /// <summary>
        /// 类名
        /// </summary>
        private readonly static string className = typeof(Progress).Name;
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="progressLogin"></param>
        public static void LoginEdgeCore(this PublisherSocket publisher, ProgressLoginInfo progressLogin)
        {
            publisher.Send($"EdgeCore/{className}/Login", progressLogin);
        }
        /// <summary>
        /// 发送心跳
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="clientId"></param>
        public static void HeartBeat(this PublisherSocket publisher, string clientId)
        {
            publisher.Send($"EdgeCore/{className}/HeartBeat", clientId);
        }
    }
}
