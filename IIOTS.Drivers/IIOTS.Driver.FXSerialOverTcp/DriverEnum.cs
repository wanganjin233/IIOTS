using System.ComponentModel;

namespace IIOTS.Driver
{
    /// <summary>
    /// 寄存器地址类型
    /// </summary>
    public enum AddressTypeEnum
    {
        /// <summary>
        /// 辅助寄存器 
        /// </summary>   
        [Description("Bit")]
        M = 0x100,
        /// <summary>
        /// 特殊继电器
        /// </summary>  
        [Description("Bit")]
        SM = -0x208,
        /// <summary>
        /// 输入继电器 
        /// </summary> 
        [Description("BitHex")]
        X = 0x80,
        /// <summary>
        /// 输出继电器 
        /// </summary> 
        [Description("BitHex")]
        Y = 0xA0,
        /// <summary>
        /// 步进继电器
        /// </summary>
        [Description("Bit")]
        S = 0x0,
        /// <summary>
        /// 定时器触点 
        /// </summary> 
        [Description("Bit")]
        TS = 0xC0, 
        /// <summary>
        /// 定时器当前值 
        /// </summary> 
        [Description("Bit")]
        TC = 0x2C0, 
        /// <summary>
        /// 计数器触点
        /// </summary>
        [Description("Bit")]
        CS = 0x1C0,
        /// <summary>
        /// 计数器线圈
        /// </summary>
        [Description("Bit")]
        CC = 0x3C0, 
        /// <summary>
        /// 特殊寄存器
        /// </summary> 
        [Description("Word")]
        SD = -0x3080,
        /// <summary>
        /// 数据寄存器
        /// </summary>
        [Description("Word")]
        D = 0x1000,
        /// <summary>
        /// 计数器当前值
        /// </summary> 
        [Description("Word")]
        CN = 0xA00
    }

}