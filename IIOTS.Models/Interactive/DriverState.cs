namespace IIOTS.Models
{
    public class DriverInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string? State { get; set; }
        /// <summary>
        /// 情况
        /// </summary>
        public string? Status { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public Dictionary<string, string>? IPAddress { get; set; }
        /// <summary>
        /// 运行
        /// </summary>
        public bool Run { get; set; } = false;

    }
}
