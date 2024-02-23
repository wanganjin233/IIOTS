using IIOTS.Enum;
using IIOTS.Interface;
using IIOTS.Util;
using System.IO.Ports;

namespace IIOTS.Communication
{
    public class SerialPort : ICommunication
    {
        /// <summary>
        /// 串口对象
        /// </summary>  
        private System.IO.Ports.SerialPort? serialPort;
        /// <summary>
        /// 连接状态
        /// </summary>
        public bool Connected => serialPort?.IsOpen ?? false;
        /// <summary>
        /// 串口名
        /// </summary>
        private readonly string _PortName = string.Empty;
        /// <summary>
        /// 波特率
        /// </summary>
        private readonly int _BaudRate = 9600;
        /// <summary>
        /// 数据位
        /// </summary>
        private readonly int _DataBits = 8;
        /// <summary>
        /// 奇偶数
        /// </summary>
        private readonly Parity _Parity = Parity.None;
        /// <summary>
        /// 停止位
        /// </summary>
        private readonly StopBits _StopBits = StopBits.One;
        /// <summary>
        /// 公开串口名
        /// </summary>
        public string PortName => _PortName;
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
        private byte[] ReceiveBuffer { set; get; } = Array.Empty<byte>();
        /// <summary>
        /// 发送超时
        /// </summary>
        public int SendTimeout { get; set; } = 0;
        /// <summary>
        /// 接收超时
        /// </summary>
        public int ReceiveTimeout { get; set; } = 0;
        /// <summary>
        /// 长度补充
        /// </summary>
        public int LengthReplenish { set; get; } = 0;
        /// <summary>
        /// 连接中
        /// </summary>
        private bool _connecting = false;
        /// <summary>
        /// 锁
        /// </summary>
        private object _lock = new object();
        public SerialPort(string portName, int baudRate, int dataBits, string parity, string stopBits)
        {
            _PortName = portName;
            _BaudRate = baudRate;
            _DataBits = dataBits;
            _Parity = parity switch
            {
                "奇数" => _Parity = Parity.Odd,
                "偶数" => _Parity = Parity.Even,
                "标记" => _Parity = Parity.Mark,
                "空格" => _Parity = Parity.Space,
                _ => _Parity = Parity.None
            };
            _StopBits = stopBits switch
            {
                "1" => StopBits.One,
                "1.5" => StopBits.OnePointFive,
                "2" => StopBits.Two,
                _ => StopBits.None
            };
        }
        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public void Connect()
        {
            try
            {
                Close(); 
                serialPort = new System.IO.Ports.SerialPort(_PortName, _BaudRate, _Parity, _DataBits, _StopBits)
                {
                    DtrEnable = true,
                    RtsEnable = true,
                    ReadTimeout = ReceiveTimeout,
                    WriteTimeout = SendTimeout
                };
                serialPort.Open();

            }
            catch (Exception)
            {

            }
        }
        /// <summary>
        /// 接收
        /// </summary>
        /// <returns></returns>
        public byte[]? Receive()
        {
            if (Connected && serialPort != null)
            {
                return this.ReceiveProcess(serialPort, ReceiveBuffer)?.FirstOrDefault();
            }
            return null;
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
                if (Connected && serialPort != null)
                {
                    serialPort.Write(buffer, 0, buffer.Length);
                    success = true;
                }
                else
                {
                    Connect();
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
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            serialPort?.Close();
            serialPort?.Dispose();
            serialPort = null;
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }
    }
}