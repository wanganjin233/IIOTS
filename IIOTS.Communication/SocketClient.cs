using IIOTS.Enum;
using IIOTS.Interface;
using IIOTS.Util;
using System.Net.Sockets;

namespace IIOTS.Communication
{
    public class SocketClient : ICommunication
    {
        /// <summary>
        /// 连接套接字
        /// </summary> 
        private Socket? clientSocket;
        /// <summary>
        /// 锁
        /// </summary>
        private object _lock = new object();
        /// <summary>
        /// 头字节
        /// </summary>
        public byte[] HeadBytes { get; set; } = Array.Empty<byte>();
        /// <summary>
        /// 尾字节
        /// </summary>
        public byte[] EndBytes { get; set; } = Array.Empty<byte>();
        /// <summary>
        /// 数据长度位置
        /// </summary>
        public int DataLengthLocation { get; set; } = -1;

        /// <summary>
        /// 数据长度类型
        /// </summary>
        public LengthTypeEnum DataLengthType { get; set; } = LengthTypeEnum.Byte; 
        /// <summary>
        /// 缓存
        /// </summary>
        private byte[] ReceiveBuffer { set; get; } = [];
        /// <summary>
        /// 长度补充
        /// </summary>
        public int LengthReplenish { set; get; } = 0;
        /// <summary>
        /// 心跳定时器
        /// </summary>
        private Timer? Timer = null;
        /// <summary>
        /// 连接方法
        /// </summary>
        private readonly Func<Socket> connFunc;
        /// <summary>
        /// 接收委托
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="bytes"></param>
        public delegate void ReceiveDelegate(Socket socket, byte[] bytes);
        /// <summary>
        /// 接收事件
        /// </summary>
        public event ReceiveDelegate? ReceiveEvent;
        /// <summary>
        /// 连接委托
        /// </summary>
        /// <param name="socket"></param> 
        public delegate void ConnectDelegate(Socket socket);
        /// <summary>
        /// 连接事件
        /// </summary>
        public event ConnectDelegate? ConnectEvent;
        /// <summary>
        /// 断开连接委托
        /// </summary>
        /// <param name="socket"></param> 
        public delegate void DisconnectDelegate();
        /// <summary>
        /// 断开连接事件
        /// </summary>
        public event DisconnectDelegate? DisconnectEvent;
        /// <summary>
        /// 连接状态
        /// </summary>
        public bool Connected => clientSocket?.Connected ?? false;
        /// <summary>
        /// 发送超时时间
        /// </summary>
        public int SendTimeout { get; set; } = -1;
        /// <summary>
        /// 接收超时时间
        /// </summary>
        public int ReceiveTimeout { get; set; } = -1;
        /// <summary>
        /// 心跳周期
        /// </summary>
        public int HeartbeatCycle { get; set; } = 5000;
        /// <summary>
        /// 心跳包
        /// </summary>
        public byte[]? HeartbeatBytes { get; set; }
        /// <summary>
        /// 登陆包
        /// </summary>
        public byte[]? LoginBytes { get; set; }
        /// <summary>
        /// 连接中
        /// </summary>
        private bool _connecting = false;
        /// <summary>
        /// 初始化连接
        /// </summary>
        /// <param name="ConnectionString">连接字符串</param> 
        public SocketClient(string ConnectionString, bool unix = false)
        {
            if (unix)
            {
                connFunc = () =>
                {
                    Socket socket = new(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP)
                    {
                        SendTimeout = SendTimeout,
                        ReceiveTimeout = ReceiveTimeout
                    };
                    socket.Connect(new UnixEndPoint(ConnectionString));
                    return socket;
                };
            }
            else
            {
                connFunc = () =>
                {
                    string[] ConnectionStrings = ConnectionString.Split(":");
                    if (ConnectionStrings.Length == 2)
                    {
                        Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                        {
                            SendTimeout = SendTimeout,
                            ReceiveTimeout = ReceiveTimeout,
                            
                        };
                        socket.Connect(ConnectionStrings[0], int.Parse(ConnectionStrings[1])); 
                        return socket;
                    }
                    throw new Exception("连接字符串格式错误！");
                };
            }
        }
        /// <summary>
        /// 连接
        /// </summary> 
        /// <returns></returns>
        public void Connect()
        {
            if (_connecting)
                return;
            lock (_lock)
            {
                _connecting = true;
                try
                {
                    Close();
                    if (Timer == null && HeartbeatBytes?.Length > 0)
                    {
                        Timer = new Timer((o) => SendConformity(HeartbeatBytes), null, HeartbeatCycle, HeartbeatCycle);
                    }
                    clientSocket = connFunc.Invoke(); 
                    Task.Delay(100).Wait();
                    if (Connected)
                    {
                        ConnectEvent?.Invoke(clientSocket);
                        if (ReceiveEvent != null)
                        {
                            ReceiveLoop();
                        }
                        if (LoginBytes?.Length > 0)
                        {
                            SendConformity(new byte[] { 00 }.AddBytes(LoginBytes).AddBytes(new byte[] { 01 }));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"连接异常【{e.Message}】");

                }
                _connecting = false;
            }
        }
        /// <summary>
        /// 接收触发
        /// </summary>
        /// <returns></returns>
        public void ReceiveLoop()
        {
            _ = Task.Factory.StartNew(() =>
            {
                //接收缓存
                while (Connected && clientSocket != null)
                {
                    var bytes = this.ReceiveProcess(clientSocket, ReceiveBuffer);
                    if (bytes != null)
                    {
                        foreach (var bufferItem in bytes)
                        {
                            ReceiveEvent?.Invoke(clientSocket, bufferItem);
                        }
                    }
                    else
                    {
                        Close();
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 接收
        /// </summary>
        /// <returns></returns>
        public byte[]? Receive()
        {
            if (Connected && clientSocket != null)
            {
                return this.ReceiveProcess(clientSocket, ReceiveBuffer)?.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 发送到客户端带报头尾
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool SendConformity(byte[] bytes)
        {
            byte[]? lengthData = (DataLengthType) switch
            {

                LengthTypeEnum.UShort => BitConverter.GetBytes((ushort)bytes.Length),
                LengthTypeEnum.ReUShort => BitConverter.GetBytes((ushort)bytes.Length).Reverse().ToArray(),
                LengthTypeEnum.Uint => BitConverter.GetBytes((uint)bytes.Length),
                LengthTypeEnum.HUint => BitConverter.GetBytes((uint)bytes.Length).HiloExchange(),
                LengthTypeEnum.ReUint => BitConverter.GetBytes((uint)bytes.Length).Reverse().ToArray(),
                LengthTypeEnum.ReHUint => BitConverter.GetBytes((uint)bytes.Length).HiloExchange()?.Reverse().ToArray(),
                _ => new byte[1] { (byte)bytes.Length }
            };
            if (lengthData != null)
            {
                byte[] datas = Array.Empty<byte>(); ;
                datas = datas.AddBytes(HeadBytes).AddBytes(lengthData).AddBytes(bytes).AddBytes(EndBytes);
                return Send(datas);
            }
            return false;
        }
        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public bool Send(byte[] buffer)
        {
            bool success = false;
            try
            {
                if (!Connected)
                {
                    Connect();
                }
                if (Connected && clientSocket != null)
                {
                    int len = clientSocket.Send(buffer);
                    success = buffer.Length == len;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Connect();
            }
            return success;
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            Close();
            Timer?.Dispose();
        }
        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            clientSocket?.Shutdown(SocketShutdown.Both);
            clientSocket?.Close();
            clientSocket?.Dispose();
            clientSocket = null;
            ReceiveBuffer = [];
            DisconnectEvent?.Invoke();
            GC.Collect();
        }
    }
}