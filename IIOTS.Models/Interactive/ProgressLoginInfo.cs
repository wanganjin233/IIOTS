namespace IIOTS.Models
{
    public class ProgressLoginInfo
    {
        /// <summary>
        /// 启动时间
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.UtcNow.AddHours(8);
        /// <summary>
        /// 心跳时间
        /// </summary>
        public DateTime HeartbeatTime { get; set; } = DateTime.MinValue;
        /// <summary>
        /// 客户端类型
        /// </summary>
        public string ClientType { get; set; } = string.Empty;
        /// <summary>
        /// 客户端Id
        /// </summary>
        public string ClientId => $"{Name}_{ClientType}";
        /// <summary>
        /// 配置名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 设备驱动配置信息
        /// </summary>
        public ProgressConfig? progressConfig { get; set; }
    }
}
