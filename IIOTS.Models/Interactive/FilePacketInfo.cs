namespace IIOTS.Models
{
    public class FilePacketInfo
    {
        /// <summary>
        /// 文件名字
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        /// <summary>
        /// 包号
        /// </summary>
        public long PacketNumber { get; set; }
        /// <summary>
        /// 总数
        /// </summary>
        public long PacketCount { get; set; }
        /// <summary>
        /// md5
        /// </summary>
        public string MD5 { get; set; } = string.Empty; 
        /// <summary>
        /// 数据包
        /// </summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}
