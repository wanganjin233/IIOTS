namespace IIOTS.Driver
{
    /// <summary>
    /// 指令协议生成 
    /// </summary>
    public static class ModbusRtuCommand
    {
        /// <summary>
        /// 生成批量读取指令 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="addressType"></param>
        /// <param name="length"></param>
        /// <param name="networkNumber"></param>
        /// <param name="networkStationNumber"></param>
        /// <returns></returns>
        internal static byte[] BatchReadCommand(this ushort address, ushort length, byte addressType, byte stationNumber)
        {
            var addBuffer = BitConverter.GetBytes(address);
            var readLenBuffer = BitConverter.GetBytes(length);
            byte[] commandBytes = new byte[8];
            commandBytes[0] = stationNumber;
            commandBytes[1] = addressType;
            commandBytes[2] = addBuffer[1];
            commandBytes[3] = addBuffer[0];
            commandBytes[4] = readLenBuffer[1];
            commandBytes[5] = readLenBuffer[0];
            return commandBytes.CRC16Calc();
        }
        /// <summary>
        /// 生成批量写入指令 
        /// 01 10 00 00 00 05 0A 00 0A 00 14 00 1E 00 28 00 32 82 86
        /// 01 0F 00 00 00 05 01 1D AF 5F
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
            var readLenBuffer = BitConverter.GetBytes(isBit ? value.Length : value.Length / 2);
            byte length = (byte)(isBit ? value.Length / 8 + 1 : value.Length);
            byte[] commandBytes = new byte[9 + length];
            commandBytes[0] = stationNumber;
            commandBytes[1] = (byte)(isBit ? 0x0F : 0x10);
            commandBytes[2] = addBuffer[1];
            commandBytes[3] = addBuffer[0];
            commandBytes[4] = readLenBuffer[1];
            commandBytes[5] = readLenBuffer[0];
            commandBytes[6] = length;
            if (isBit)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (BitConverter.ToBoolean(value, i))
                    {
                        commandBytes[7 + i / 8] = (byte)(commandBytes[7 + i / 8] | 0x01 << i);
                    }
                }
                return commandBytes.CRC16Calc();
            }
            else
            {
                value.CopyTo(commandBytes, 7);
                return commandBytes.CRC16Calc();
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


        private static byte[] CRC16Calc(this byte[] dataBuff)
        {
            if (dataBuff.Length > 2)
            {
                int CRCResult = 0xFFFF;
                for (int i = 0; i < dataBuff.Length - 2; i++)
                {
                    CRCResult = CRCResult ^ dataBuff[i];
                    for (int j = 0; j < 8; j++)
                    {
                        if ((CRCResult & 1) == 1)
                            CRCResult = CRCResult >> 1 ^ 0xA001;
                        else CRCResult >>= 1;
                    }
                }
                dataBuff[dataBuff.Length - 1] = Convert.ToByte(CRCResult >> 8);
                dataBuff[dataBuff.Length - 2] = Convert.ToByte(CRCResult & 0xff);
            }
            return dataBuff;
        }
    }
}
