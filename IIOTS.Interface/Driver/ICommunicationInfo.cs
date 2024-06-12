using IIOTS.Enums;

namespace IIOTS.Interface
{
    public interface ICommunicationInfo
    {  
        /// <summary>
        /// 头字节
        /// </summary>
        public byte[] HeadBytes { get; set; }
        /// <summary>
        /// 结束字节
        /// </summary>
        public byte[] EndBytes { get; set; }
        /// <summary>
        /// 数据长度位置
        /// </summary>
        public int DataLengthLocation { get; set; }
        /// <summary>
        /// 长度补充
        /// </summary>
        public int LengthReplenish { get; set; }
        /// <summary>
        /// 数据长度类型
        /// </summary>
        public LengthTypeEnum DataLengthType { get; set; }
        /// <summary>
        /// 发送超时时间
        /// </summary>
        public int SendTimeout { get; set; }  
        /// <summary>
        /// 接收超时时间
        /// </summary>
        public int ReceiveTimeout { get; set; }  
    }
}
