namespace IIOTS.Driver
{
    /// <summary>
    /// 指令协议生成 
    /// </summary>
    public static class MC3ECommand
    {
        /*************读协议内容******************************
         * 副标题(2)50 00|网络编号(1)00|PLC编号(1)FF
         * IO编号(2)FF 03|站编号(1)00|请求数据长度(2)_12 
         * 应答超时(2)1000|命令(2)_|子命令(2)_|起始地址(3)_
         * 请求软元件代码(1)|请求点数长度(2)
         * **************************************************
         示例指令 ：50 00 00 FF FF 03 00 0C 00 01 00 01 04 00 00 A6 27 00 A8 01 00（读取D10150开始的1个数据块）*/
        /// <summary>
        /// 生成批量读取指令 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="addressType"></param>
        /// <param name="length"></param>
        /// <param name="networkNumber"></param>
        /// <param name="networkStationNumber"></param>
        /// <returns></returns>
        internal static byte[] BatchReadCommand(this uint address, AddressTypeEnum addressType, ushort length, bool isBit, byte networkNumber, byte networkStationNumber)
        {
            var addBuffer = BitConverter.GetBytes(address);
            var readLenBuffer = BitConverter.GetBytes(length);
            byte[] commandBytes = new byte[21];
            commandBytes[0] = 0x50;
            commandBytes[1] = 0x0;
            commandBytes[2] = networkNumber;
            commandBytes[3] = 0xFF;
            commandBytes[4] = 0xFF;
            commandBytes[5] = 0x3;
            commandBytes[6] = networkStationNumber;
            commandBytes[7] = 0xC;
            commandBytes[8] = 0x0;
            commandBytes[9] = 0xA;
            commandBytes[10] = 0x0;
            commandBytes[11] = 0x1;
            commandBytes[12] = 0x4;
            commandBytes[13] = (byte)(isBit ? 1 : 0);
            commandBytes[14] = 0x0;
            commandBytes[15] = addBuffer[0];
            commandBytes[16] = addBuffer[1];
            commandBytes[17] = addBuffer[2];
            commandBytes[18] = (byte)addressType;
            commandBytes[19] = readLenBuffer[0];
            commandBytes[20] = readLenBuffer[1];
            return commandBytes;
        }
        /// <summary>
        /// 生成批量写入指令
        /// 50 00 00 FF FF 03 00 
        /// 0E 00 指令长度
        /// 0A 00 超时 25*10
        /// 01 14 写入
        /// 00 00 子/ 01 00 位
        /// 64 00 00 起始地址
        /// 90       地址类型
        /// 01 00 /长度
        /// 79 00 /值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="addressType"></param>
        /// <param name="value"></param>
        /// <param name="networkNumber"></param>
        /// <param name="networkStationNumber"></param>
        /// <returns></returns>
        internal static byte[] BatchWriteCommand(this uint address, AddressTypeEnum addressType, byte[] value, bool isBit, byte networkNumber = 0, byte networkStationNumber = 0)
        {

            var addBuffer = BitConverter.GetBytes(address);
            var lenBuffer = BitConverter.GetBytes(12 + value.Length);

            byte[] commandBytes = new byte[21 + value.Length];
            commandBytes[0] = 0x50;
            commandBytes[1] = 0x0;
            commandBytes[2] = networkNumber;
            commandBytes[3] = 0xFF;
            commandBytes[4] = 0xFF;
            commandBytes[5] = 0x3;
            commandBytes[6] = networkStationNumber;
            commandBytes[7] = lenBuffer[0];
            commandBytes[8] = lenBuffer[1];
            commandBytes[9] = 0xA;
            commandBytes[10] = 0x0;
            commandBytes[11] = 0x1;
            commandBytes[12] = 0x14;
            commandBytes[13] = (byte)(isBit ? 1 : 0);
            commandBytes[14] = 0x0;
            commandBytes[15] = addBuffer[0];
            commandBytes[16] = addBuffer[1];
            commandBytes[17] = addBuffer[2];
            commandBytes[18] = (byte)addressType;

            var writeLenBuffer = BitConverter.GetBytes(value.Length / (isBit ? 1 : 2));
            commandBytes[19] = writeLenBuffer[0];
            commandBytes[20] = writeLenBuffer[1];
            if (isBit) for (int i = 0; i < value.Length; i++)
                {
                    commandBytes[21 + i / 2] += (byte)(i % 2 == 0 ? value[i] * 16 : value[i]);
                }
            else
                value.CopyTo(commandBytes, 21);
            return commandBytes;
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
