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
    }
}
