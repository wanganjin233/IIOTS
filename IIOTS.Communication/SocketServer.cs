using IIOTS.Enum;
using IIOTS.Interface;
using IIOTS.Util;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace IIOTS.Communication
{
    public class SocketServer : ICommunicationInfo, IDisposable
    {
        /// <summary>
        /// 是否需要登录
        /// </summary>
        public bool LoginAsk { get; set; } = false;
        /// <summary>
        /// 最大连接数
        /// </summary>
        private int MaxConnect { get; set; }
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
        /// 长度补充
        /// </summary>
        public int LengthReplenish { set; get; } = 0;
        /// <summary>
        /// 接收委托
        /// </summary>
        /// <param name="client"></param>
        /// <param name="bytes"></param>
        public delegate void ReceiveDelegate(string ClientId, byte[] bytes);
        /// <summary>
        /// 接收事件
        /// </summary>
        public event ReceiveDelegate? ReceiveEvent;
        /// <summary>
        /// 断开委托
        /// </summary>
        /// <param name="client"></param>
        /// <param name="bytes"></param>
        public delegate void DisconnectDelegate(string ClientId, Socket socket);
        /// <summary>
        /// 断开事件
        /// </summary>
        public event DisconnectDelegate? DisconnectEvent;
        /// <summary>
        /// 连接委托
        /// </summary>
        /// <param name="client"></param>
        /// <param name="bytes"></param>
        public delegate void ConnectDelegate(string ClientId, Socket socket);
        /// <summary>
        /// 连接事件
        /// </summary>
        public event ConnectDelegate? ConnectEvent;
        /// <summary>
        /// 客户端列表
        /// </summary> 
        protected ConcurrentDictionary<string, Socket> Sockets = new();
        /// <summary>
        /// 心跳包
        /// </summary>
        public byte[]? HeartbeatBytes { get; set; }
        /// <summary>
        /// 发送超时时间
        /// </summary>
        public int SendTimeout { get; set; } = 1000;
        /// <summary>
        /// 接收超时时间
        /// </summary>
        public int ReceiveTimeout { get; set; } = 10000;
        private readonly Socket ServerSocket;
        private readonly EndPoint _EndPoint;
        public void Start()
        {
            ServerSocket.Bind(_EndPoint);
            ServerSocket.Listen(1000);
            ServerSocket.BeginAccept(AcceptCallback, ServerSocket);
        }

        /// <summary>
        /// 创建服务
        /// </summary>
        /// <param name="port">端口</param>
        /// <param name="ip">地址</param>
        /// <param name="maxConnect">最大连接数</param>
        public SocketServer(int port, int maxConnect = 10)
        {
            MaxConnect = maxConnect;
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _EndPoint = new IPEndPoint(IPAddress.Any, port);

        }
        /// <summary>
        /// 创建服务
        /// </summary>
        /// <param name="port">端口</param>
        /// <param name="ip">地址</param>
        /// <param name="maxConnect">最大连接数</param>
        public SocketServer(string unixPath, int maxConnect = 10)
        {
            MaxConnect = maxConnect;
            ServerSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            _EndPoint = new UnixEndPoint(unixPath);
        }
        /// <summary>
        /// 客户端连接回调
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                if (ar.AsyncState is Socket _Socket)
                {
                    var client = _Socket.EndAccept(ar);
                    _Socket.BeginAccept(AcceptCallback, _Socket);
                    AddClient(client);
                }
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// 新增会话方法
        /// </summary>
        /// <param name="client"></param>
        private async void AddClient(Socket socket)
        {
            await Task.Run(() =>
             {
                 if (Sockets.Count >= MaxConnect)
                 {
                     socket.Send(Encoding.UTF8.GetBytes("超出服务器连接数上限"));
                     Console.WriteLine("超出服务器连接数上限");
                     CloseClient(socket);
                 }
                 else
                 {
                     socket.ReceiveTimeout = ReceiveTimeout;
                     socket.SendTimeout = SendTimeout;
                     string? ClientId;
                     if (LoginAsk)
                     {
                         byte[] ReceiveBuffer = Array.Empty<byte>();
                         var bytes = this.ReceiveProcess(socket, ReceiveBuffer)?.FirstOrDefault();
                         if (bytes != null && bytes.First() == 00 && bytes.Last() == 01)
                         {
                             ClientId = Encoding.UTF8.GetString(bytes.Skip(1).SkipLast(1).ToArray());
                         }
                         else
                         {
                             Console.WriteLine("登陆失败");
                             CloseClient(socket);
                             return;
                         }
                     }
                     else
                     {
                         ClientId = Guid.NewGuid().ToString("N");
                     }
                     ReceiveLoop(ClientId, socket);
                     Sockets.TryAdd(ClientId, socket);
                     ConnectEvent?.Invoke(ClientId, socket);
                 }
             });
        }
        /// <summary>
        /// 踢出会话
        /// </summary>
        /// <param name="client"></param>
        public void RemoveClient(string ClientId)
        {
            if (Sockets.Remove(ClientId, out Socket? socket))
            {
                DisconnectEvent?.Invoke(ClientId, socket);
                CloseClient(socket);
            }
        }
        /// <summary>
        /// 关闭会话
        /// </summary>
        /// <param name="client"></param>
        private void CloseClient(Socket socket)
        {
            if (socket.Connected)
                socket?.Disconnect(false);
            socket?.Close();
            socket?.Dispose();
            GC.Collect();
        }
        /// <summary>
        /// 接收触发
        /// </summary>
        /// <returns></returns>
        public void ReceiveLoop(string ClientId, Socket socket)
        {
            _ = Task.Factory.StartNew(() =>
            {
                byte[] ReceiveBuffer = Array.Empty<byte>();
                //接收缓存
                while (socket.Connected)
                {
                    var bytes = this.ReceiveProcess(socket, ReceiveBuffer);
                    if (bytes != null)
                    {
                        foreach (var bufferItem in bytes)
                        {
                            if (bufferItem.Equalsbytes(HeartbeatBytes))
                            {
                                SendConformity(ClientId, bufferItem);
                            }
                            else
                            {
                                ReceiveEvent?.Invoke(ClientId, bufferItem);
                            }
                        }
                    }
                    else
                    {
                        RemoveClient(ClientId);
                        break;
                    }
                }
            },TaskCreationOptions.LongRunning);
        }
        /// <summary>
        /// 发送到指定客户端带报头尾
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool SendConformity(string ClientId, byte[] bytes)
        {
            byte[]? lengthData = DataLengthType switch
            {
                LengthTypeEnum.UShort => BitConverter.GetBytes((ushort)bytes.Length),
                LengthTypeEnum.ReUShort => BitConverter.GetBytes((ushort)bytes.Length).Reverse().ToArray(),
                LengthTypeEnum.Uint => BitConverter.GetBytes((uint)bytes.Length),
                LengthTypeEnum.HUint => BitConverter.GetBytes((uint)bytes.Length).HiloExchange(),
                LengthTypeEnum.ReUint => BitConverter.GetBytes((uint)bytes.Length).Reverse().ToArray(),
                LengthTypeEnum.ReHUint => BitConverter.GetBytes((uint)bytes.Length).HiloExchange()?.Reverse().ToArray(),
                _ => new byte[1] { (byte)bytes.Length },
            };
            if (lengthData != null)
            {
                byte[] datas = Array.Empty<byte>(); ;
                datas = datas.AddBytes(HeadBytes).AddBytes(lengthData).AddBytes(bytes).AddBytes(EndBytes);
                return Send(ClientId, datas);
            }
            return false;
        }
        /// <summary>
        /// 发送到指定客户端带报头尾
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public void SendConformity(byte[] bytes)
        {
            byte[]? lengthData = (DataLengthType) switch
            {
                LengthTypeEnum.UShort => BitConverter.GetBytes((ushort)bytes.Length),
                LengthTypeEnum.ReUShort => BitConverter.GetBytes((ushort)bytes.Length).Reverse().ToArray(),
                LengthTypeEnum.Uint => BitConverter.GetBytes((uint)bytes.Length),
                LengthTypeEnum.HUint => BitConverter.GetBytes((uint)bytes.Length).HiloExchange(),
                LengthTypeEnum.ReUint => BitConverter.GetBytes((uint)bytes.Length).Reverse().ToArray(),
                LengthTypeEnum.ReHUint => BitConverter.GetBytes((uint)bytes.Length).HiloExchange()?.Reverse().ToArray(),
                _ => new byte[1] { (byte)bytes.Length },
            };
            if (lengthData != null)
            {
                byte[] datas = Array.Empty<byte>(); ;
                datas = datas.AddBytes(HeadBytes).AddBytes(lengthData).AddBytes(bytes).AddBytes(EndBytes);
                Send(datas);
            }
        }
        /// <summary>
        /// 发送到指定客户端
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool Send(string ClientId, byte[] bytes)
        {
            bool success = false;
            try
            {
                Sockets.TryGetValue(ClientId, out Socket? socket);
                if (socket != null && socket.Connected)
                {
                    int len = socket.Send(bytes);
                    success = bytes.Length == len;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return success;
        }
        /// <summary>
        /// 发送到所有客户端
        /// </summary>
        /// <param name="bytes"></param> 
        public void Send(byte[] bytes)
        {
            foreach (var key in Sockets.Keys)
            {
                ThreadPool.QueueUserWorkItem(p => Send(key, bytes));
            }
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            while (!Sockets.IsEmpty)
            {
                if (Sockets.Remove(Sockets.Keys.First(), out Socket? socket))
                {
                    if (socket.Connected)
                        socket?.Disconnect(false);
                    socket?.Close();
                    socket?.Dispose();
                }
            }
            if (ServerSocket.Connected)
                ServerSocket?.Disconnect(false);
            ServerSocket?.Close();
            ServerSocket?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
