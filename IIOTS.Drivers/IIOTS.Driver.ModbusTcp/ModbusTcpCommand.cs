namespace IIOTS.Driver
{
    /// <summary>
    /// 指令协议生成 
    /// </summary>
    public static class ModbusTcpCommand
    {
        /// <summary>
        /// 生成批量读取指令 
        /// 00 00 00 00 00 06 02 03 00 64 00 01
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <param name="addType"></param>
        /// <param name="stationNumber"></param>
        /// <returns></returns>
        internal static byte[] BatchReadCommand(this ushort address,   ushort length, byte addType, byte stationNumber)
        {  
            var addBuffer = BitConverter.GetBytes(address);
            var readLenBuffer = BitConverter.GetBytes(length);
            byte[] commandBytes = new byte[12];
            commandBytes[5] = 0x06;
            commandBytes[6] = stationNumber;
            commandBytes[7] = addType;
            commandBytes[8] = addBuffer[1];
            commandBytes[9] = addBuffer[0];
            commandBytes[10] = readLenBuffer[1];
            commandBytes[11] = readLenBuffer[0];
            return commandBytes;
        }
        /// <summary>
        /// 生成批量写入指令 
        /// 00 01 00 00 00 09 01 10 00 00 00 01 02 00 0F
        /// 00 01 00 00 00 06 01 10 00 00 00 01
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="addressType"></param>
        /// <param name="value"></param>
        /// <param name="networkNumber"></param>
        /// <param name="networkStationNumber"></param>
        /// <returns></returns>
        internal static byte[] BatchWriteCommand(this ushort address, byte[] value, bool isBit, byte stationNumber)
        {
            var addBuffer = BitConverter.GetBytes(address);
            byte length = (byte)(isBit ? value.Length / 8 + 1 : value.Length);
            var valueLength = BitConverter.GetBytes((ushort)(isBit ? value.Length : value.Length / 2));
            byte[] commandBytes = new byte[13 + length];
            commandBytes[5] = (byte)(length + 7);//字节长度
            commandBytes[6] = stationNumber;//站号
            commandBytes[7] = (byte)(isBit ? 0x0f : 0x10);//线圈或寄存器
            commandBytes[8] = addBuffer[1];//开始地址
            commandBytes[9] = addBuffer[0];// 
            commandBytes[10] = valueLength[1];//长度
            commandBytes[11] = valueLength[0];//
            commandBytes[12] = length;//byte长度
            if (isBit)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (BitConverter.ToBoolean(value, i))
                    {
                        commandBytes[13 + i / 8] = (byte)(commandBytes[13 + i / 8] | (0x01 << i));
                    }
                }
                return commandBytes;
            }
            else
            {
                value.CopyTo(commandBytes, 13);
                return commandBytes;
            }
        }
        /// <summary>
        /// 生成随机读取指令
        /// </summary>
        /// <param name="Addresses"></param>
        /// <returns></returns>
        internal static byte[] RandomReadCommand(this List<string> Addresses) { return Array.Empty<byte>(); }
        /// <summary>
        /// 生成随机写入指令
        /// </summary>
        /// <param name="Addresses"></param>
        /// <returns></returns>
        internal static byte[] RandomWriteCommand(this Dictionary<string, object> Addresses) { return Array.Empty<byte>(); }
    }
}
