using IIOTS.Enum;
using IIOTS.Interface;
using System.IO.Ports;
using System.Net.Sockets;

namespace IIOTS.Util
{
    /// <summary>
    /// 拓展类
    /// </summary>
    public static partial class Extension
    {
        /// <summary>
        /// 接收委托
        /// </summary>
        /// <returns></returns>
        private delegate byte[] Receive();
        /// <summary>
        /// socket接收字符处理
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="communicationInfo"></param>
        /// <returns></returns>
        public static List<byte[]>? ReceiveProcess(this ICommunicationInfo communicationInfo, Socket socket, byte[] receiveBuffer)
        {
            byte[] receive()
            {
                byte[] buffer = new byte[socket.ReceiveBufferSize];
                int len = socket.Receive(buffer);
                return buffer.Take(len).ToArray();
            }
            return ReceiveProcess(receive, communicationInfo, receiveBuffer);
        }
        /// <summary>
        /// 串口接收字符处理
        /// </summary>
        /// <param name="communicationInfo"></param>
        /// <param name="serialPort"></param>
        /// <returns></returns>
        public static List<byte[]>? ReceiveProcess(this ICommunicationInfo communicationInfo, SerialPort serialPort, byte[] receiveBuffer)
        {
            byte[] receive()
            {
                byte[] buffer = new byte[serialPort.ReadBufferSize];
                int len = serialPort.Read(buffer, 0, buffer.Length);
                return buffer.Take(len).ToArray();
            }
            return ReceiveProcess(receive, communicationInfo, receiveBuffer);
        }

        /// <summary>
        /// 流处理
        /// </summary>
        /// <returns></returns>
        private static List<byte[]>? ReceiveProcess(Receive receive, ICommunicationInfo communicationInfo, byte[] receiveBuffer)
        {
            try
            {
                int DataLengthType = communicationInfo.DataLengthType switch
                {
                    LengthTypeEnum.Byte => 1,
                    LengthTypeEnum.UShort => 2,
                    LengthTypeEnum.ReUShort => 2,
                    LengthTypeEnum.Uint => 4,
                    LengthTypeEnum.HUint => 4,
                    LengthTypeEnum.ReHUint => 4,
                    LengthTypeEnum.ReUint => 4,
                    _ => throw new NotImplementedException()
                };
                List<byte[]> buffers = new();
                while (true)
                {
                    //接收阻塞 
                    byte[] buffer = receive();
                    if (!buffer.Any())
                        return null;//断开信号 
                    receiveBuffer = receiveBuffer.AddBytes(buffer);
                    while (true)
                    {
                        int headIndex = receiveBuffer.IndexOf(communicationInfo.HeadBytes[0]);
                        if (headIndex == -1)
                        {
                            receiveBuffer = Array.Empty<byte>();
                            break;
                        }
                        else
                        {
                            receiveBuffer = receiveBuffer.Skip(headIndex).ToArray();
                            int headBytesIndex = receiveBuffer.IndexOf(communicationInfo.HeadBytes);
                            if (headBytesIndex > -1)
                            {
                                receiveBuffer = receiveBuffer.Skip(headBytesIndex).ToArray();
                                int length = communicationInfo.DataLengthType switch
                                {
                                    LengthTypeEnum.Byte => receiveBuffer[communicationInfo.DataLengthLocation],
                                    LengthTypeEnum.UShort => BitConverter.ToUInt16(receiveBuffer, communicationInfo.DataLengthLocation),
                                    LengthTypeEnum.ReUShort => BitConverter.ToUInt16(receiveBuffer
                                                                                    .Skip(communicationInfo.DataLengthLocation)
                                                                                    .Take(2).Reverse()
                                                                                    .ToArray()),
                                    LengthTypeEnum.Uint => BitConverter.ToInt32(receiveBuffer, communicationInfo.DataLengthLocation),
                                    LengthTypeEnum.HUint => BitConverter.ToInt32(receiveBuffer
                                                                                    .Skip(communicationInfo.DataLengthLocation)
                                                                                    .Take(4)
                                                                                    .ToArray()
                                                                                    .HiloExchange()),
                                    LengthTypeEnum.ReUint => BitConverter.ToInt32(receiveBuffer
                                                                                    .Skip(communicationInfo.DataLengthLocation)
                                                                                    .Take(4)
                                                                                    .Reverse()
                                                                                    .ToArray()),
                                    LengthTypeEnum.ReHUint => BitConverter.ToInt32(receiveBuffer
                                                                                    .Skip(communicationInfo.DataLengthLocation)
                                                                                    .Take(4)
                                                                                    .Reverse()
                                                                                    .ToArray()
                                                                                    .HiloExchange()),
                                    _ => throw new NotImplementedException()
                                } + communicationInfo.LengthReplenish;
                                if (receiveBuffer.Length >= communicationInfo.HeadBytes.Length + DataLengthType + length + communicationInfo.EndBytes.Length)
                                {
                                    byte[] bytes = receiveBuffer
                                            .Skip(communicationInfo.HeadBytes.Length + DataLengthType)
                                            .Take(length).ToArray();
                                    if (receiveBuffer.Equalsbytes(communicationInfo.EndBytes, communicationInfo.HeadBytes.Length + DataLengthType + length))
                                    {
                                        receiveBuffer = receiveBuffer
                                            .Skip(communicationInfo.DataLengthLocation + DataLengthType + length)
                                            .ToArray();
                                        buffers.Add(bytes);
                                    }
                                    else
                                    {
                                        receiveBuffer = receiveBuffer
                                            .Skip(communicationInfo.HeadBytes.Length)
                                            .ToArray();
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                if (receiveBuffer.Length > communicationInfo.HeadBytes.Length)
                                    receiveBuffer = receiveBuffer
                                        .TakeLast(communicationInfo.HeadBytes.Length)
                                        .ToArray();
                                break;
                            }
                        }
                    }
                    if (buffers.Any())
                    {
                        return buffers;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
