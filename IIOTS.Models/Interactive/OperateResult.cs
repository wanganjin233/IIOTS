namespace IIOTS.Models
{
    /// <summary>
    /// 操作结果的泛型类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OperateResult<T> 
    {  
        /// <summary>
        /// 指示消息代码
        /// </summary>
        public string MessageCode { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// 路由
        /// </summary>
        public string Router { get; set; } = string.Empty;
        /// <summary>
        /// 发送端标识
        /// </summary>
        public string? SenderIdentity { get; set; }  
        /// <summary>
        /// 接收端标识
        /// </summary>
        public string? ReceiverIdentity { get; set; }
        /// <summary>
        /// 指示本次访问是否成功
        /// </summary>
        public bool IsSuccess { get; set; } = true; 
    
        /// <summary>
        /// 具体的描述
        /// </summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// 具体的错误代码
        /// </summary>
        public int ErrorCode { get; set; }
        /// <summary>
        /// 用户自定义的泛型数据
        /// </summary>
        public T? Content { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime TimeSpan { get; set; } = DateTime.UtcNow;
    }
}
