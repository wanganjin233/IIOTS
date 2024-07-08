namespace IIOTS.Driver
{
    /// <summary>
    /// 指令协议生成 
    /// </summary>
    public static class XINJIECommand
    {
        /// <summary>
        /// 生成批量读取指令  
        /// 00 00 00 00 00 08 01 20 80 00 00 64 00 01
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <param name="addType"></param>
        /// <param name="stationNumber"></param>
        /// <returns></returns>
        internal static byte[] BatchReadCommand(this uint address, ushort length, AddressTypeEnum addType, byte stationNumber)
        {
            var addBuffer = BitConverter.GetBytes(address);
            var readLenBuffer = BitConverter.GetBytes(length);
            var addTypeBuffer = BitConverter.GetBytes((uint)addType);
            byte[] commandBytes = new byte[14];
            commandBytes[5] = 0x08;
            commandBytes[6] = stationNumber;
            commandBytes[7] = addTypeBuffer[1];
            commandBytes[8] = addTypeBuffer[0];
            commandBytes[9] = addBuffer[2];
            commandBytes[10] = addBuffer[1];
            commandBytes[11] = addBuffer[0];
            commandBytes[12] = readLenBuffer[1];
            commandBytes[13] = readLenBuffer[0];
            return commandBytes;
        }
        /// <summary>
        /// 生成批量写入指令 
        /// 00 0F 00 00 00 0B 00 21 80 00 1E 02 00 01 02 00 70
        /// 00 0F 00 00 00 08 00 21 80 00 1E 02 00 01 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="addressType"></param>
        /// <param name="value"></param>
        /// <param name="networkNumber"></param>
        /// <param name="networkStationNumber"></param>
        /// <returns></returns>
        internal static byte[] BatchWriteCommand(this uint address, byte[] value, object addressType, bool isBit, byte stationNumber)
        {
            var addTypeBuffer = BitConverter.GetBytes((int)addressType);
            var addBuffer = BitConverter.GetBytes(address);
            byte length = (byte)(isBit ? value.Length / 8 + 1 : value.Length);
            var valueLength = BitConverter.GetBytes((ushort)(isBit ? value.Length : value.Length / 2));
            byte[] commandBytes = new byte[15 + length];
            commandBytes[5] = (byte)(length + 9);//字节长度
            commandBytes[6] = stationNumber;//站号
            commandBytes[7] = ++addTypeBuffer[1];
            commandBytes[8] = addTypeBuffer[0];
            commandBytes[9] = addBuffer[2];//开始地址
            commandBytes[10] = addBuffer[1];
            commandBytes[11] = addBuffer[0];
            commandBytes[12] = valueLength[1];//长度
            commandBytes[13] = valueLength[0];//
            commandBytes[14] = length;//byte长度
            if (isBit)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (BitConverter.ToBoolean(value, i))
                    {
                        commandBytes[15 + i / 8] = (byte)(commandBytes[15 + i / 8] | (0x01 << i));
                    }
                }
                return commandBytes;
            }
            else
            {
                value.CopyTo(commandBytes, 15);
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
