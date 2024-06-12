using IIOTS.Enums;

namespace IIOTS.Models
{
    public class HandlerResult
    {
        public MsgTypeEnum MsgType { get; set; }
        /// <summary>
        /// 消息ID
        /// </summary>
        public string? MsgCode { get; set; }
        /// <summary>
        /// 回复路由
        /// </summary>
        public string? Router { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public object? Data { get; set; }
    }
}
