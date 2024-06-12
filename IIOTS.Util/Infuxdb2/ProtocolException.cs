using System;

namespace IIOTS.Util.Infuxdb2
{
    /// <summary>
    /// 协议异常
    /// </summary>
    public class ProtocolException : Exception
    {
        /// <summary>
        /// 协议异常
        /// </summary>
        /// <param name="message">提示消息</param>
        public ProtocolException(string message)
            : base(message)
        {
        }
    }
}
