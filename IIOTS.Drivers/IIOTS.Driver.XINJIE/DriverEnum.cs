using System.ComponentModel;
namespace IIOTS.Driver
{

    /// <summary>
    /// 寄存器地址类型
    /// </summary>
    public enum AddressTypeEnum
    {
        /// <summary>
        /// 内部寄存器
        /// </summary> 
        [Description("Bit")]
        M = 0x1E03,
        /// <summary>
        /// 输入线圈
        /// </summary> 
        [Description("BitHex")]
        X = 0x2001,
        /// <summary>
        /// 输出线圈
        /// </summary> 
        [Description("BitHex")]
        Y = 0x2002,
        /// <summary>
        /// 流程继电器
        /// </summary>
        S = 0x2004,
        /// <summary>
        /// 特殊继电器
        /// </summary>
        HS = 0x2009,
        /// <summary>
        /// 定时器
        /// </summary>
        T = 0x2005,
        /// <summary>
        /// 计数器
        /// </summary>
        C = 0x2006,
        /// <summary>
        /// 精确定时器
        /// </summary>
        ET = 0x2007,
        /// <summary>
        /// 内部继电器
        /// </summary>
        HM = 0x2008,
        /// <summary>
        /// 定时器
        /// </summary>
        HT = 0x200A,
        /// <summary>
        /// 计数器
        /// </summary>
        HC = 0x200B,
        /// <summary>
        /// 高速计数器
        /// </summary>
        HSC = 0x200C,
        /// <summary>
        /// 数据寄存器
        /// </summary> 
        [Description("Word")]
        D = 0x2080,
        /// <summary>
        /// 本体扩展模块
        /// </summary> 
        ID = 0x2086,
        /// <summary>
        /// 本体扩展模块
        /// </summary> 
        QD = 0x2087,
        /// <summary>
        /// 特殊寄存器
        /// </summary> 
        SD = 0x2083,
        /// <summary>
        /// 定时器当前值
        /// </summary> 
        TD = 0x2081,
        /// <summary>
        /// 计数器当前值
        /// </summary> 
        CD = 0x2082,
        /// <summary>
        /// 精确定时器当前值
        /// </summary> 
        ETD = 0x2085,
        /// <summary>
        /// 数据寄存器
        /// </summary> 
        [Description("Word")]
        HD = 0x2088,
        /// <summary>
        /// 特殊寄存器
        /// </summary> 
        HSD = 0x208C,
        /// <summary>
        /// 定时器当前值
        /// </summary> 
        HTD = 0x2089,
        /// <summary>
        /// 计数器当前值
        /// </summary> 
        HCD = 0x208A,
        /// <summary>
        /// 高速计数器当前值
        /// </summary> 
        HSCD = 0x208B,
        /// <summary>
        /// FlaashROM寄存器
        /// </summary> 
        FD = 0x208D,
        /// <summary>
        /// 特殊FlaashROM寄存器
        /// </summary> 
        SFD = 0x208E,
    }
}
