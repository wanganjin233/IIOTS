namespace IIOTS.Driver
{
    internal static class S7Command
    {
        /// <summary>
        /// 一次握手
        /// </summary>
        internal static byte[] FirstHandshake =
        [
            0x03,
            0x00,
            0x00,
            0x16,
            0x11,
            0xE0,
            0x00,
            0x00,
            0x00,
            0x01,
            0x00,
            0xC0,
            0x01,
            0x0A,
            0xC1,
            0x02,
            0x01,
            0x02,
            0xC2,
            0x02,
            0x01,
            0x00
        ];
        /// <summary>
        ///二次握手 
        /// </summary>
        internal static byte[] SecondHandshake =
        [
            0x03,
            0x00,
            0x00,
            0x19,
            0x02,
            0xF0,
            0x80,
            0x32,
            0x01,
            0x00,
            0x00,
            0x04,
            0x00,
            0x00,
            0x08,
            0x00,
            0x00,
            0xF0,
            0x00,
            0x00,
            0x01,
            0x00,
            0x01,
            0x01,
            0xE0
        ];
        /// <summary>
        /// 生成批量读取指令 
        /// </summary>
        /// <param name="address"></param> 
        /// <returns></returns>
        internal static byte[] BatchReadCommand(this S7Addresses[] address, ushort msgId = 1)
        {

            byte[] commandBytes = new byte[address.Length * 12 + 19];
            //固定头
            commandBytes[0] = 0x03;
            commandBytes[1] = 0x00;
            //报文长度
            commandBytes[2] = (byte)(commandBytes.Length / 256);
            commandBytes[3] = (byte)(commandBytes.Length % 256);
            //固定标识
            commandBytes[4] = 0x02;
            commandBytes[5] = 0xF0;
            commandBytes[6] = 0x80;
            commandBytes[7] = 0x32;
            //发送命令
            commandBytes[8] = 0x01;
            //预留
            commandBytes[9] = 0x00;
            commandBytes[10] = 0x00;
            commandBytes[11] = BitConverter.GetBytes(msgId)[1];
            commandBytes[12] = BitConverter.GetBytes(msgId)[0];
            //参数长度
            commandBytes[13] = (byte)((commandBytes.Length - 17) / 256);
            commandBytes[14] = (byte)((commandBytes.Length - 17) % 256);
            //数据长度
            commandBytes[15] = 0x00;
            commandBytes[16] = 0x00;
            //读取功能码
            commandBytes[17] = 0x04;
            //读取数据块个数
            commandBytes[18] = (byte)address.Length;
            for (int i = 0; i < address.Length; i++)
            {
                //有效值类型
                commandBytes[19 + i * 12] = 0x12;
                //地址长度
                commandBytes[20 + i * 12] = 0x0A;
                //syntaxId
                commandBytes[21 + i * 12] = 0x10;
                //传输类型
                commandBytes[22 + i * 12] = 0x02;
                //请求长度
                commandBytes[23 + i * 12] = (byte)(address[i].Length / 256);
                commandBytes[24 + i * 12] = (byte)(address[i].Length % 256);
                //DB号 
                commandBytes[25 + i * 12] = (byte)(address[i].DbBlock / 256);
                commandBytes[26 + i * 12] = (byte)(address[i].DbBlock % 256);
                //存储区类型
                commandBytes[27 + i * 12] = (byte)address[i].AddressType;
                //开始字节
                var Address1 = address[i].Address * 8;
                commandBytes[28 + i * 12] = (byte)(Address1 / 256 / 256 % 256);
                commandBytes[29 + i * 12] = (byte)(Address1 / 256 % 256);
                commandBytes[30 + i * 12] = (byte)(Address1 % 256);
            }
            return commandBytes;
        }

        /// <summary>
        /// 生成批量写入命令
        /// </summary>
        /// <param name="address"></param> 
        /// <returns></returns>
        internal static byte[] BatchWriteCommand(this S7Addresses[] address, ushort msgId = 1)
        {
            MemoryStream memoryStream = new();
            memoryStream.Write(
            [
                //固定头
                0x03,
                0x00,
                //报文长度
                0,
                0,
                //固定标识
                0x02,
                0xF0,
                0x80,
                0x32,
                0x01
            ]);
            //预留
            memoryStream.Write(
            [
                0x00,
                0x00,
                BitConverter.GetBytes(msgId)[1],
                BitConverter.GetBytes(msgId)[0]
            ]);
            //参数长度
            memoryStream.Write(
            [
                BitConverter.GetBytes(address.Length * 12 + 2)[1],
                BitConverter.GetBytes(address.Length * 12 + 2)[0]
            ]);
            //数据长度
            memoryStream.Write(
            [
                0x00,
                0x00
            ]);
            //功能码05写入
            memoryStream.WriteByte(0x05);
            //读取数据块个数
            memoryStream.WriteByte((byte)address.Length);
            for (int i = 0; i < address.Length; i++)
            {
                memoryStream.Write([
                    //指定有效值类型
                    0x12,
                    //本次地址访问长度
                    0x0A,
                    //语法标记ANY
                    0x10
                    ]
                    );
                //按字为单位02，按位为单位01
                memoryStream.WriteByte((byte)(address[i].IsBit ? 0x01 : 0x02));
                //访问数据的字节个数
                byte[]? writeData = address[i].WriteData;
                int WriteDataLength = writeData?.Length ?? 0;
                memoryStream.Write([
                    BitConverter.GetBytes(WriteDataLength)[1],
                    BitConverter.GetBytes(WriteDataLength)[0]
                ]);
                //DB块编号
                memoryStream.Write([
                    BitConverter.GetBytes(address[i].DbBlock)[1],
                    BitConverter.GetBytes(address[i].DbBlock)[0]
                ]);

                //M区为0x83，DB块为0x84
                memoryStream.WriteByte((byte)address[i].AddressType);
                //地址
                uint AddressStart = (uint)(address[i].Address * 8 + (address[i].IsBit ? address[i].BitLocation : 0));
                memoryStream.WriteByte(BitConverter.GetBytes(AddressStart)[2]);
                memoryStream.WriteByte(BitConverter.GetBytes(AddressStart)[1]);
                memoryStream.WriteByte(BitConverter.GetBytes(AddressStart)[0]);

            }
            int num = (int)memoryStream.Length;
            for (int i = 0; i < address.Length; i++)
            {
                //按字写入0x00 0x04,按位写入0x00 0x03
                memoryStream.Write([
                    0x00,
                    (byte)(address[i].IsBit?0x03:0x04)]);

                byte[]? writeData = address[i].WriteData;
                if (writeData != null)
                {
                    int writeDataLength = address[i].IsBit ? address[i].Length : address[i].Length * 8;
                    //写入的数据长度
                    memoryStream.Write([
                        BitConverter.GetBytes(writeDataLength)[1],
                        BitConverter.GetBytes(writeDataLength)[0]
                        ]);
                    memoryStream.Write(writeData); 
                }
                else
                {
                    memoryStream.Write([0x00, 0x00]);
                }
            }
            byte[] bytes = memoryStream.ToArray();
            bytes[2] = (byte)(bytes.Length / 256);
            bytes[3] = (byte)(bytes.Length % 256);
            bytes[15] = BitConverter.GetBytes(bytes.Length - num)[1];
            bytes[16] = BitConverter.GetBytes(bytes.Length - num)[0];
            return bytes;
        }
    }
}
