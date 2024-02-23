using System.Net.Sockets;

namespace IIOTS.Models
{
    public class SocketClientInfo
    {
        /// <summary>
        /// socket客户
        /// </summary>
        public Socket? Socket { get; set; }
        /// <summary>
        /// 连接段进程Id
        /// </summary>
        public int ProgressId { get; set; } =-1;
        /// <summary>
        /// 客户端类型
        /// </summary>
        public string ClientType { get; set; } = string.Empty;
        /// <summary>
        /// 客户端Id
        /// </summary>
        public string ClientId => $"{EQU}_{ClientType}";
        /// <summary>
        /// 设备号
        /// </summary>
        public string EQU { get; set; } = string.Empty;
    }
}
