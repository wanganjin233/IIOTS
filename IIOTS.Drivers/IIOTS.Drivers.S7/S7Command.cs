

using IIOTS.Models;
using NetMQ;

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
        internal static byte[] BatchReadCommand(this S7Addresses[] address, ushort msgId)
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
        internal static byte[] BatchWriteCommand(this S7Addresses[] address)
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
            commandBytes[11] = 0x00;
            commandBytes[12] = 0x01;
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
                commandBytes[27 + i * 12] = 0x84;
                //开始字节
                var Address1 = address[i].Address * 8;
                commandBytes[28 + i * 12] = (byte)(Address1 / 256 / 256 % 256);
                commandBytes[29 + i * 12] = (byte)(Address1 / 256 % 256);
                commandBytes[30 + i * 12] = (byte)(Address1 % 256);
            }
            return commandBytes;
        }
    }
}
