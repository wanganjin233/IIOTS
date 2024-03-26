namespace IIOTS.Driver
{
    /// <summary>
    /// 寄存器地址类型
    /// </summary>
    public enum AddressTypeEnum
    {
        /// <summary>
        /// 输入寄存器
        /// </summary> 
        I = 0x81,
        /// <summary>
        /// 输出寄存器
        /// </summary> 
        Q = 0x82,
        /// <summary>
        ///内部寄存器
        /// </summary> 
        M = 0x83,
        /// <summary>
        /// 数据寄存器
        /// </summary> 
        DB = 0x84,
        /// <summary>
        /// 系统寄存器 0
        /// </summary> 
        SM = 0x05,
        /// <summary>
        /// 外设寄存器
        /// </summary> 
        P = 0x80,
        /// <summary>
        /// 定时寄存器
        /// </summary> 
        T = 0x1F,
        /// <summary>
        /// 计数器寄存器
        /// </summary> 
        C = 0x1E,
        /// <summary>
        /// 智能输入寄存器
        /// </summary> 
        AI = 0x06,
        /// <summary>
        /// 智能输出寄存器
        /// </summary> 
        AQ = 0x07
    }
}