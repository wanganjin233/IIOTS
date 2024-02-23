namespace IIOTS.Driver
{
    internal static class FinsCommand
    {
        /// <summary>
        /// 登录指令包
        /// </summary>
        internal static byte[] LogInCommand => new byte[20]
        {
            0x46,
            0x49,
            0x4E,
            0x53,
            0x00,
            0x00,
            0x00,
            0x0C,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00
        };
        /// <summary>
        /// 生成批量读取指令 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="addressType"></param>
        /// <param name="length"></param>
        /// <param name="networkNumber"></param>
        /// <param name="networkStationNumber"></param>
        /// <returns></returns>
        internal static byte[] BatchReadCommand(this ushort address, AddressTypeEnum addressType, ushort length, bool isBit, byte StationNumber = 0, byte? PLCNode = null, byte? PCNode = null, byte bitAddress = 0)
        {

            byte[] commandBytes = new byte[34];
            commandBytes[0] = 0x46;
            commandBytes[1] = 0x49;
            commandBytes[2] = 0x4E;
            commandBytes[3] = 0x53;

            //长度 
            commandBytes[4] = 0x00;
            commandBytes[5] = 0x00;
            commandBytes[6] = 0x00;
            commandBytes[7] = 0x1A;

            //固定命令
            commandBytes[8] = 0x00;
            commandBytes[9] = 0x00;
            commandBytes[10] = 0x00;
            commandBytes[11] = 0x02;

            commandBytes[12] = 0x00;
            commandBytes[13] = 0x00;
            commandBytes[14] = 0x00;
            commandBytes[15] = 0x00;

            commandBytes[16] = 0x80;//ICF
            commandBytes[17] = 0x00;//RSV
            commandBytes[18] = 0x02;//GCT
            commandBytes[19] = StationNumber;//PLC网络地址

            commandBytes[20] = PLCNode ?? 0;//PLC节点地址

            commandBytes[21] = 0x00;//PLC单元地址 
            commandBytes[22] = 0x00;//PC网络地址 
            commandBytes[23] = PCNode ?? 0;//PC节点地址 
            commandBytes[24] = 0x00;//PC单元地址 
            commandBytes[25] = 0x00;//SID 

            //读取
            commandBytes[26] = 0x01;
            commandBytes[27] = 0x01;
            //类型
            commandBytes[28] = isBit ? (byte)(addressType - 0x80) : (byte)addressType;

            byte[] addressByte = BitConverter.GetBytes(address);

            //地址
            commandBytes[29] = addressByte[1];
            commandBytes[30] = addressByte[0];
            commandBytes[31] = isBit ? bitAddress : (byte)0x00;

            byte[] readLength = BitConverter.GetBytes(length);

            commandBytes[32] = readLength[1];
            commandBytes[33] = readLength[0];
            return commandBytes;
        }

        /// <summary>
        /// 生成批量写入命令
        /// </summary>
        /// <param name="address"></param>
        /// <param name="addressType"></param>
        /// <param name="value"></param>
        /// <param name="isBit"></param>
        /// <param name="networkNumber"></param>
        /// <param name="networkStationNumber"></param>
        /// <returns></returns>
        internal static byte[] BatchWriteCommand(this ushort address, AddressTypeEnum addressType, byte[] value, bool isBit, byte? PLCNode = null, byte? PCNode = null, byte StationNumber = 0, byte bitAddress = 0)
        {
            byte[] commandBytes = new byte[34 + value.Length];
            commandBytes[0] = 0x46;
            commandBytes[1] = 0x49;
            commandBytes[2] = 0x4E;
            commandBytes[3] = 0x53;

            //长度 
            var lenBuffer = BitConverter.GetBytes(0x1A + value.Length);
            commandBytes[4] = 0x00;
            commandBytes[5] = 0x00;
            commandBytes[6] = lenBuffer[1];
            commandBytes[7] = lenBuffer[0];

            //固定命令
            commandBytes[8] = 0x00;
            commandBytes[9] = 0x00;
            commandBytes[10] = 0x00;
            commandBytes[11] = 0x02;

            commandBytes[12] = 0x00;
            commandBytes[13] = 0x00;
            commandBytes[14] = 0x00;
            commandBytes[15] = 0x00;

            commandBytes[16] = 0x80;//ICF
            commandBytes[17] = 0x00;//RSV
            commandBytes[18] = 0x02;//GCT
            commandBytes[19] = StationNumber;//PLC网络地址

            commandBytes[20] = PLCNode ?? 0;//PLC节点地址

            commandBytes[21] = 0x00;//PLC单元地址 
            commandBytes[22] = 0x00;//PC网络地址 
            commandBytes[23] = PCNode ?? 0;//PC节点地址 
            commandBytes[24] = 0x00;//PC单元地址 
            commandBytes[25] = 0x00;//SID 

            //读取
            commandBytes[26] = 0x01;
            commandBytes[27] = 0x02;
            //类型
            commandBytes[28] = isBit ? (byte)(addressType - 0x80) : (byte)addressType;

            //地址
            byte[] addressByte = BitConverter.GetBytes(address);
            commandBytes[29] = addressByte[1];
            commandBytes[30] = addressByte[0];
            commandBytes[31] = isBit ? bitAddress : (byte)0x00;

            byte[] readLength = BitConverter.GetBytes(isBit ? value.Length : value.Length / 2);
            commandBytes[32] = readLength[1];
            commandBytes[33] = readLength[0];
            if (isBit) for (int i = 0; i < value.Length; i++)
                {
                    commandBytes[34 + i] = value[i];
                }
            else
                value.CopyTo(commandBytes, 34);
            return commandBytes;
        }
    }
}
