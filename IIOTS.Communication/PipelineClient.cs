using System.IO.Pipes;
using System.Security.Principal;
using System.Text;

namespace IIOTS.Communication
{
    public class PipelineClient : IDisposable
    {
        private string _identifier = Guid.NewGuid().ToString("N");
        /// <summary>
        /// 连接服务端标识
        /// </summary>
        public string Identifier => _identifier;
        /// <summary>
        /// 连接状态
        /// </summary>
        public bool Connected => pipeClient?.IsConnected ?? false;
        /// <summary>
        /// 锁
        /// </summary>
        private object _lock = new();
        /// <summary>
        /// 管道
        /// </summary> 
        NamedPipeClientStream? pipeClient;
        /// <summary>
        /// 管道名称
        /// </summary> 
        public string PipelineName { get; }
        /// <summary>
        /// 连接中
        /// </summary>
        private bool _connecting = false;
        /// <summary>
        /// 连接方法
        /// </summary>
        private readonly Func<NamedPipeClientStream?> connFunc;
        StreamWriter? sw;
        StreamReader? rd;
        public PipelineClient(string name, string? login = null)
        {
            PipelineName = name;
            connFunc = () =>
            {
                pipeClient = new(".", name, PipeDirection.InOut, PipeOptions.Asynchronous, TokenImpersonationLevel.None);
                pipeClient.Connect(3000);
                pipeClient.ReadMode = PipeTransmissionMode.Byte;
                sw = new(pipeClient, Encoding.UTF8, 81920) { AutoFlush = true }; 
                rd = new(pipeClient, Encoding.UTF8, true, 81920);
                if (login != null)
                {
                    sw.WriteLine($"Login:{login}:End");
                    string? loginReturn = rd.ReadLine();
                    if (loginReturn != null && loginReturn.StartsWith("Return:") && loginReturn.EndsWith(":End"))
                    {
                        _identifier = loginReturn.Split(':')[1];
                        return pipeClient;
                    }
                    pipeClient.Close();
                    pipeClient.Dispose();
                    return null;
                }
                else
                {
                    return pipeClient;
                }

            };
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
                    pipeClient = connFunc.Invoke();
                    ReceiveLoop();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"连接异常【{e}】");
                }
            }
            _connecting = false;
        }

        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="message">信息内容</param>
        /// <param name="actionFail">发送失败回调</param>
        /// <returns></returns>
        public   bool Send(string? message, Action<string?>? actionFail = null)
        {
            bool isSucceed = false;
            if (message != null)
            {
                try
                {
                    if (!Connected)
                    {
                        Connect();
                    }
                    if (Connected)
                    {
                        sw?.WriteLine (message) ;
                        sw?.Flush();
                       isSucceed = true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            if (!isSucceed)
            {
                actionFail?.Invoke(message);
            }
            return isSucceed;
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
                while (Connected)
                {
                    try
                    {
                        var rdStr = rd?.ReadLine();
                        if (string.IsNullOrEmpty(rdStr))
                        {
                            Close();
                        }
                        else
                        {
                            ThreadPool.QueueUserWorkItem(p =>
                                    ReceiveEvent?.Invoke(this, rdStr)
                                );
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"服务端断开连接【{e.Message}】");
                        Close();
                    }
                }
            },TaskCreationOptions.LongRunning);
        }


        /// <summary>
        /// 接收委托
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="data">数据</param>
        public delegate void ReceiveDelegate(PipelineClient pipelineClient, string data);
        /// <summary>
        /// 接收事件
        /// </summary>
        public event ReceiveDelegate? ReceiveEvent;
        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            pipeClient?.Close();
            pipeClient?.Dispose();
        }
    }
}
