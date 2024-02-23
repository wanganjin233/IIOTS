using System.ComponentModel;

namespace IIOTS.Driver
{
    /// <summary>
    /// 寄存器地址类型
    /// </summary>
    public enum AddressTypeEnum
    {
        /// <summary>
        /// 输入继电器 
        /// </summary> 
        [Description("BitHex")]
        X = 0x9C,
        /// <summary>
        /// 输出继电器 
        /// </summary> 
        [Description("BitHex")]
        Y = 0x9D,
        /// <summary>
        /// 链接继电器 
        /// </summary> 
        [Description("BitHex")]
        B = 0xA0,
        /// <summary>
        /// 锁存继电器 
        /// </summary> 
        [Description("Bit")]
        L = 0x92,
        /// <summary>
        /// 报警继电器 
        /// </summary> 
        [Description("Bit")]
        F = 0x93,
        /// <summary>
        /// 边沿继电器 
        /// </summary> 
        [Description("Bit")]
        V = 0x94,
        /// <summary>
        /// 定时器触点 
        /// </summary> 
        [Description("Bit")]
        TS = 0xC1,
        /// <summary>
        /// 定时器当前值 
        /// </summary> 
        [Description("Bit")]
        TC = 0xC0,
        /// <summary>
        /// 累计定时器触点  
        /// </summary> 
        [Description("Bit")]
        SS = 0xC7,
        /// <summary>
        /// 累计定时器线圈
        /// </summary> 
        [Description("Bit")]
        SC = 0xC6,
        /// <summary>
        /// 计数器触点
        /// </summary>
        [Description("Bit")]
        CS = 0xC4,
        /// <summary>
        /// 计数器线圈
        /// </summary>
        [Description("Bit")]
        CC = 0xC3,
        /// <summary>
        /// 步进继电器
        /// </summary>
        [Description("Bit")]
        S = 0x98,
        /// <summary>
        /// 辅助寄存器 
        /// </summary>   
        [Description("Bit")]
        M = 0x90,
        /// <summary>
        /// 特殊继电器
        /// </summary>  
        [Description("Bit")]
        SM = 0x91,
        /// <summary>
        /// 特殊寄存器
        /// </summary> 
        [Description("Word")]
        SD = 0xA9,
        /// <summary>
        /// 数据寄存器
        /// </summary>
        [Description("Word")]
        D = 0xA8,
        /// <summary>
        /// 定时器线圈
        /// </summary> 
        [Description("Word")]
        TN = 0xC2,
        /// <summary>
        /// 累计定时器当前值
        /// </summary> 
        [Description("Word")]
        SN = 0xC8,
        /// <summary>
        /// 计数器当前值
        /// </summary> 
        [Description("Word")]
        CN = 0xC5,
        /// <summary>
        /// 链接寄存器
        /// </summary>
        [Description("WordHex")]
        W = 0xB4
    }

}