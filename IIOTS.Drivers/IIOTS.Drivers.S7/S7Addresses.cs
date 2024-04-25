namespace IIOTS.Driver
{
    internal class S7Addresses
    {    /// <summary>
         /// DB块数据信息
         /// </summary>
        public ushort DbBlock { get; set; }
        /// <summary>
        /// 设备地址信息
        /// </summary>
        public uint Address { get; set; }
        /// <summary>
        /// 数据长度
        /// </summary>
        public ushort Length { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public AddressTypeEnum AddressType { get; set; }
        /// <summary>
        /// 是否为位地址
        /// </summary>
        public bool IsBit { get; set; } = false;
        /// <summary>
        /// 位地址位置
        /// </summary>
        public int BitLocation { get; set; } = -1;
        /// <summary>
        /// 写入内容
        /// </summary>
        public byte[]? WriteData { get; set; }
        /// <summary>
        /// 点位信息
        /// </summary>
        public List<TagProcess> Tags { get; set; } = [];
    }
}
