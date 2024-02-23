namespace IIOTS.Interface
{
    public interface ICommunication : ICommunicationInfo, IDisposable
    {
        /// <summary>
        /// 连接
        /// </summary>
        public void Connect();
        /// <summary>
        /// 关闭
        /// </summary>
        public void Close();
        /// <summary>
        /// 连接状态
        /// </summary>
        public bool Connected { get; }
        /// <summary>
        /// 接受信息
        /// </summary>
        /// <returns></returns>
        public byte[]? Receive();
        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public bool Send(byte[] buffer);
    }
}
