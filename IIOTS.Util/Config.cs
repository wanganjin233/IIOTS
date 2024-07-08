namespace IIOTS.Util
{
    public static class Config
    {
        /// <summary>
        /// 标识
        /// </summary>
        public static string Identifier { get; set; } = string.Empty;
        private static string? localIp;
        /// <summary>
        /// 本机ip
        /// </summary>
        public static string LocalIp => localIp ??= IpHelper.GetLocalIp();
        private static readonly object _lock = new();
        private static int? publisherPort;
        /// <summary>
        /// 发布用端口
        /// </summary>
        public static int PublisherPort
        {
            get
            {
                lock (_lock)
                {
                    publisherPort ??= IpHelper.GetFirstAvailablePort([subscriberPort ?? -1]);
                    return (int)publisherPort;
                }
            }
        }

        private static int? subscriberPort;
        /// <summary>
        /// 订阅用端口
        /// </summary>
        public static int SubscriberPort
        {
            get
            {
                lock (_lock)
                {
                    subscriberPort ??= IpHelper.GetFirstAvailablePort([publisherPort ?? -1]);
                    return (int)subscriberPort;
                }
            }
        }
        /// <summary>
        /// 启用的设备配置路径
        /// </summary>
        public static string EnableConfigPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("Config", "EnableConfig"));
        /// <summary>
        /// 未启用的设备配置路径
        /// </summary>
        public static string DisabledConfigPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("Config", "DisabledConfig"));
        /// <summary>
        /// Tag配置文件路径
        /// </summary>
        public static string TagConfigPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("Config", "TagConfig"));
    }
}
