namespace IIOTS.Driver
{
    /// <summary>
    /// 寄存器地址类型
    /// </summary>
    public enum AddressTypeEnum
    {
        /// <summary>
        /// IO区
        /// </summary> 
        CIO = 0xB0,
        /// <summary>
        /// 工作区域 
        /// </summary> 
        WR = 0xB1,
        /// <summary>
        /// 保持区 
        /// </summary> 
        HR = 0xB2,
        /// <summary>
        /// IO区 
        /// </summary> 
        AR = 0xB3,
        /// <summary>
        /// 数据储存区 
        /// </summary> 
        D = 0x82,
        /// <summary>
        /// 数据储存区 0
        /// </summary> 
        E0 = 0xA0,
        /// <summary>
        /// 数据储存区 1
        /// </summary> 
        E1 = 0xA1,
        /// <summary>
        /// 数据储存区 2
        /// </summary> 
        E2 = 0xA2,
        /// <summary>
        /// 数据储存区 3
        /// </summary> 
        E3 = 0xA3,
        /// <summary>
        /// 数据储存区 4
        /// </summary> 
        E4 = 0xA4,
        /// <summary>
        /// 数据储存区 5
        /// </summary> 
        E5 = 0xA5,
        /// <summary>
        /// 数据储存区 6
        /// </summary> 
        E6 = 0xA6,
        /// <summary>
        /// 数据储存区 7
        /// </summary> 
        E7 = 0xA7,
        /// <summary>
        /// 数据储存区 8
        /// </summary> 
        E8 = 0xA8,
        /// <summary>
        /// 数据储存区 9
        /// </summary> 
        E9 = 0xA9,
        /// <summary>
        /// 数据储存区 10
        /// </summary> 
        EA = 0xAA,
        /// <summary>
        /// 数据储存区 11
        /// </summary> 
        EB = 0xAB,
        /// <summary>
        /// 数据储存区 12
        /// </summary> 
        EC = 0xAC,
        /// <summary>
        /// 数据储存区 13
        /// </summary> 
        ED = 0xAD,
        /// <summary>
        /// 数据储存区 14
        /// </summary> 
        EE = 0xAE,
        /// <summary>
        /// 数据储存区 15
        /// </summary> 
        EF = 0xAF
    }
}