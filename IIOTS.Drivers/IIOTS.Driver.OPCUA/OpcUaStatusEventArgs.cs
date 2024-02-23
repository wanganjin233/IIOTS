namespace IIOTS.Driver
{
    /// <summary>
    /// OPC UA的状态更新消息
    /// </summary>
    public class OpcUaStatusEventArgs
    {
        /// <summary>
        /// 连接是否成功
        /// </summary>
        public bool Connected { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 文本
        /// </summary>
        public string? Text { get; set; } 
    } 
}
