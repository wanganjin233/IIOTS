namespace IIOTS.Driver
{

    /// <summary>
    /// 寄存器地址类型
    /// </summary>
    public enum AddressTypeEnum
    {
        /// <summary>
        /// 输出线圈 0
        /// </summary> 
        zero = 0x1,
        /// <summary>
        /// 输入线圈 1
        /// </summary> 
        one = 0x02,
        /// <summary>
        /// 内部寄存器 3
        /// </summary> 
        threea = 0x04,
        /// <summary>
        /// 保持寄存器 4
        /// </summary> 
        four = 0x03
    }
}
