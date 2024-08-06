namespace IIOTS.Enums
{
    public enum DriverTypeEnum
    {
        /// <summary>
        /// 欧姆龙Fins协议
        /// </summary>
        Fins,
        /// <summary>
        /// 三菱MC3E协议
        /// </summary>
        MC3E,
        /// <summary>
        /// FX三菱串口转TCP
        /// </summary>
        FXSerialOverTcp, 
        /// <summary>
        /// ModbusRtu协议
        /// </summary>
        ModbusRtu,
        /// <summary>
        /// ModbusTcp协议
        /// </summary>
        ModbusTcp,
        /// <summary>
        /// OPCUA协议
        /// </summary>
        OPCUA,
        /// <summary>
        /// 西门子S7协议
        /// </summary>
        S7,
        /// <summary>
        /// 信捷协议
        /// </summary>
        XINJIE,
        /// <summary>
        /// 其他
        /// </summary>
        Other
    }
}
