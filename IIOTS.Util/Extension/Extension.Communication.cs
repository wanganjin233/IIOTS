using IIOTS.Enums;
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
                //根据长度类型确认长度字节数
                int DataLengthType = communicationInfo.DataLengthType switch
                {
                    LengthTypeEnum.Byte => 1,
                    LengthTypeEnum.UShort => 2,
                    LengthTypeEnum.ReUShort => 2,
                    LengthTypeEnum.Uint => 4,
                    LengthTypeEnum.HUint => 4,
                    LengthTypeEnum.ReHUint => 4,
                    LengthTypeEnum.ReUint => 4,
                    _ => -1
                };
                //接收的数据组
                List<byte[]> buffers = [];
                while (true)
                {
                    //接收Bytes
                    byte[] buffer = receive();
                    //长度为0断开信号 
                    if (buffer.Length == 0)
                        return null;
                    //添加接收的bytes到缓存
                    receiveBuffer = receiveBuffer.AddBytes(buffer);
                    while (true)
                    {
                        //查询头第一个字节位置
                        int headIndex = receiveBuffer.IndexOf(communicationInfo.HeadBytes[0]);
                        if (headIndex == -1)
                        {
                            //未找到头清空缓存并退出循环
                            receiveBuffer = [];
                            break;
                        }
                        else
                        {
                            //查找报文头位置
                            int headBytesIndex = receiveBuffer.IndexOf(communicationInfo.HeadBytes);
                            if (headBytesIndex > -1)
                            {
                                //找到头并删除在头报文之前的bytes
                                receiveBuffer = receiveBuffer.Skip(headBytesIndex).ToArray();
                                if (communicationInfo.DataLengthLocation < 0)
                                {
                                    //缓存长度小于长度标识报文位置和标识类型长度则跳出
                                    if (receiveBuffer.Length < communicationInfo.EndBytes.Length)
                                    {
                                        break;
                                    }
                                    int endBytesIndex = receiveBuffer.IndexOf(communicationInfo.EndBytes);
                                    if (endBytesIndex > -1 && receiveBuffer.Length <= endBytesIndex + communicationInfo.EndBytes.Length + communicationInfo.LengthReplenish)
                                    {
                                        byte[] bytes = receiveBuffer
                                                .Skip(headBytesIndex)
                                                .Take(endBytesIndex + communicationInfo.EndBytes.Length + communicationInfo.LengthReplenish).ToArray();
                                        receiveBuffer = receiveBuffer
                                           .Skip(endBytesIndex + communicationInfo.EndBytes.Length + communicationInfo.LengthReplenish)
                                           .ToArray();
                                        buffers.Add(bytes);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    //缓存长度小于长度标识报文位置和标识类型长度则跳出
                                    if (receiveBuffer.Length < communicationInfo.DataLengthLocation + DataLengthType)
                                    {
                                        break;
                                    }
                                    //根据类型报文长度标识位置获取报文长度加 补充长度（如Rtu校验，未计算在长度内）
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
                                    //接收的长度大于或等于 报文长度标识位置+数据长度字节数+报文长度标识+尾字节长度则开始解析报文内容
                                    if (receiveBuffer.Length >= communicationInfo.DataLengthLocation + DataLengthType + length + communicationInfo.EndBytes.Length)
                                    {
                                        //校验尾字节
                                        if (receiveBuffer.Equalsbytes(communicationInfo.EndBytes, communicationInfo.DataLengthLocation + DataLengthType + length))
                                        {
                                            //截取报文内容
                                            byte[] bytes = receiveBuffer
                                                    .Skip(communicationInfo.DataLengthLocation + DataLengthType)
                                                    .Take(length).ToArray();
                                            //校验通过删除报文内容
                                            receiveBuffer = receiveBuffer
                                                .Skip(communicationInfo.DataLengthLocation + DataLengthType + length + communicationInfo.EndBytes.Length)
                                                .ToArray();
                                            buffers.Add(bytes);
                                        }
                                        else
                                        {
                                            //不通过则跳过头部分
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
                            }
                            else
                            {
                                //未找到报文头并且缓存长度大于报文头则截取报文头长度缓存
                                if (receiveBuffer.Length > communicationInfo.HeadBytes.Length)
                                {
                                    receiveBuffer = receiveBuffer
                                        .TakeLast(communicationInfo.HeadBytes.Length)
                                        .ToArray();
                                }
                                break;
                            }
                        }
                    }
                    if (buffers.Count != 0)
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
