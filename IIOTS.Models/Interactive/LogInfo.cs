namespace IIOTS.Models
{
    public class LogInfo
    {
        /// <summary>
        /// 种类名称
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
        /// <summary>
        /// 日志级别
        /// </summary>
        public int LogLevel { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public object? State { get; set; }
        /// <summary>
        /// 日志内容
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
