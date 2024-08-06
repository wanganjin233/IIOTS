using IIOTS.Util;
using System.Text;

namespace IIOTS.Driver
{
    /// <summary>
    /// 指令协议生成 
    /// </summary>
    public static class FXSerialOverTcpCommand
    {
        private static ushort AddressComputation(ushort address, AddressTypeEnum addressType, bool isBit)
        {
            if (addressType == AddressTypeEnum.CN)
            {
                address = (address < 200) ? ((ushort)(address * 2 + addressType)) : ((ushort)((address - 200) * 4 + 3072));
            }
            else if (isBit)
            {
                address = (ushort)(address / 8 + addressType);
            }
            else
            {
                address = (ushort)(address * 2 + addressType);
            }
            return address;
        }


        /*************读协议内容******************************
         * 开始(1)02
         * 命令(1)00
         * PLC地址(4)地址位*2 + 4096 acsll
         * 读取长度(2)30 32
         * 结束(1)03
         * 和(2)命令到结束
         * **************************************************
         示例指令 ：02 30 31 30 43 38 30 32 03 37 31（读取D100开始的1个数据块）*/
        /// <summary>
        /// 生成批量读取指令 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="addressType"></param>
        /// <param name="length"></param>
        /// <param name="networkNumber"></param>
        /// <param name="networkStationNumber"></param>
        /// <returns></returns>
        internal static byte[] BatchReadCommand(this ushort address, AddressTypeEnum addressType, byte length, bool isBit)
        {
            if (!isBit)
                length *= 2;
            address = AddressComputation(address, addressType, isBit);
            var addBuffer = Encoding.ASCII.GetBytes(address.ToString("X4"));
            var readLenBuffer = Encoding.ASCII.GetBytes(length.ToString("X2"));
            byte[] commandBytes = new byte[11];
            commandBytes[0] = 0x02;
            commandBytes[1] = 0x30;
            commandBytes[2] = addBuffer[0];
            commandBytes[3] = addBuffer[1];
            commandBytes[4] = addBuffer[2];
            commandBytes[5] = addBuffer[3];
            commandBytes[6] = readLenBuffer[0];
            commandBytes[7] = readLenBuffer[1];
            commandBytes[8] = 0x03;
            int num = 0;
            for (int i = 1; i < commandBytes.Length - 2; i++)
            {
                num += commandBytes[i];
            }
            var CRC = Encoding.ASCII.GetBytes(((byte)num).ToString("X2"));
            commandBytes[9] = CRC[0];
            commandBytes[10] = CRC[1];
            return commandBytes;
        }
        /*************写协议内容******************************
          * 开始(1)02
          * 命令(1)01写/07置位/08复位
          * PLC地址(4)地址位*2 + 4096 acsll
          * 长度(2)30 32
          * 结束(1)03
          * 数据(N*4)
          * 和(2)命令到结束
          * **************************************************
          示例指令 ：02 31 31 30 46 36 30 34 30 31 30 30 30 30 30 30 03 46 36（写入D100,值为1）*/
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="addressType"></param>
        /// <param name="value"></param>
        /// <param name="networkNumber"></param>
        /// <param name="networkStationNumber"></param>
        /// <returns></returns>
        internal static byte[] BatchWriteCommand(this ushort address, AddressTypeEnum addressType, byte[] value, bool isBit )
        {
            address = AddressComputation(address, addressType, isBit);
            var addBuffer = Encoding.ASCII.GetBytes(address.ToString("X4"));
            byte[] commandBytes;
            if (isBit)
            {
                commandBytes = new byte[11];
                commandBytes[0] = 0x02;
                if (Convert.ToBoolean(value))
                {
                    commandBytes[1] = 0x37;
                }
                else
                {
                    commandBytes[1] = 0x38;
                }
                commandBytes[2] = addBuffer[0];
                commandBytes[3] = addBuffer[1];
                commandBytes[4] = addBuffer[2];
                commandBytes[5] = addBuffer[3];
                commandBytes[6] = 0x03;
            }
            else
            {
                var readLenBuffer = Encoding.ASCII.GetBytes(value.Length.ToString("X2"));
                commandBytes = new byte[11+ value.Length*2];
                commandBytes[0] = 0x02; 
                commandBytes[1] = 0x31;
                commandBytes[2] = addBuffer[0];
                commandBytes[3] = addBuffer[1];
                commandBytes[4] = addBuffer[2];
                commandBytes[5] = addBuffer[3];
                commandBytes[6] = readLenBuffer[0];
                commandBytes[7] = readLenBuffer[1];
                value.To0XString().ToBytes().CopyTo(commandBytes, 8);
                commandBytes[^3] = 0x03;
            } 
            int num = 0;
            for (int i = 1; i < commandBytes.Length - 2; i++)
            {
                num += commandBytes[i];
            }
            var CRC = Encoding.ASCII.GetBytes(((byte)num).ToString("X2"));
            commandBytes[^2] = CRC[0];
            commandBytes[^1] = CRC[1]; 
            return commandBytes; 
        }
        /// <summary>
        /// 生成随机读取指令
        /// </summary>
        /// <param name="Addresses"></param>
        /// <returns></returns>
        internal static byte[] RandomReadCommand(this List<string> Addresses) { return []; }
        /// <summary>
        /// 生成随机写入指令
        /// </summary>
        /// <param name="Addresses"></param>
        /// <returns></returns>
        internal static byte[] RandomWriteCommand(this Dictionary<string, object> Addresses) { return []; }
    }
}
