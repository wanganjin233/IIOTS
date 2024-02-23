using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipes;
using System.Net.NetworkInformation;
using System.Text;

namespace IIOTS.Communication
{
    public class PipelineServer
    {
        /// <summary>
        /// 管道id
        /// </summary>
        public List<string> PipelineIds => ServerPool.Keys.ToList();
        /// <summary>
        /// 用于存储和管理管道
        /// </summary>
        private readonly ConcurrentDictionary<string, Pipeline> ServerPool = new();
        /// <summary>
        /// 最大连接数量
        /// </summary>
        private readonly int MaxServer;
        /// <summary>
        /// 管道名称
        /// </summary>
        public string PipeName { get; }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="PipeName"></param>
        /// <param name="MaxServer"></param>
        public PipelineServer(string PipeName, int MaxServer = 100)
        {
            this.MaxServer = MaxServer;
            this.PipeName = PipeName;
            ThreadPool.QueueUserWorkItem(p => CreatePipeLine());
        }
        /// <summary>
        /// 创建一个新的管道
        /// </summary>
        private void CreatePipeLine()
        {
            if (ServerPool.Count < MaxServer)
            {
                Pipeline pipeline = new(PipeName, MaxServer);
                ThreadPool.QueueUserWorkItem(p =>
                {
                    pipeline.WaitForConnection();
                    ThreadPool.QueueUserWorkItem(x => CreatePipeLine());
                    try
                    {
                        string? rd = pipeline.ReadLine();
                        if (rd != null && rd.StartsWith("Login:") && rd.EndsWith(":End"))
                        {
                            string user = rd.Split(':')[1];
                            if (ServerPool.TryAdd(user, pipeline))
                            {
                                pipeline.WriteLine($"Return:{Process.GetCurrentProcess().Id}:End");
                                ConnectEvent?.Invoke(user);
                                Console.WriteLine($"管道池添加新管道【{user}】 当前管道总数{ServerPool.Count}");
                                while (true)
                                {
                                    try
                                    {
                                        rd = pipeline.ReadLine();
                                        if (rd == null)
                                            break;
                                        ThreadPool.QueueUserWorkItem(p => ReceiveEvent?.Invoke(this, user, rd));
                                    }
                                    catch (Exception)
                                    {
                                        break;
                                    }
                                }
                                DisposablePipeLine(user);
                            }
                            else
                            {
                                Console.WriteLine($"管道用户已存在");
                                pipeline.Dispose();
                            }
                        }
                        else
                        {
                            Console.WriteLine($"管道登陆失败");
                            pipeline.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"管道异常【{e.Message}】");
                        pipeline.Dispose();
                    }
                });

            }
            else
            {
                Console.WriteLine($"管道池已满");
            }
        }
        /// <summary>
        /// 发送信息到指定客户端
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="message"></param>
        public bool Send(string guid, string? message, Action<string, string?>? actionFail = null)
        {
            bool isSucceed = false;
            if (message != null)
            {
                try
                {
                    if (ServerPool.TryGetValue(guid, out var pipe))
                        isSucceed = pipe.WriteLine(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            if (!isSucceed)
            {
                ThreadPool.QueueUserWorkItem(p => actionFail?.Invoke(guid, message));
            }
            return isSucceed;
        }
        /// <summary>
        /// 接收委托
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="data">数据</param>
        public delegate void ReceiveDelegate(PipelineServer pipelineServer, string id, string data);
        /// <summary>
        /// 接收事件
        /// </summary>
        public event ReceiveDelegate? ReceiveEvent;

        /// <summary>
        /// 连接委托
        /// </summary> 
        public delegate void ConnectDelegate(string id);
        /// <summary>
        /// 连接事件
        /// </summary>
        public event ConnectDelegate? ConnectEvent;

        /// <summary>
        /// 断开委托
        /// </summary> 
        public delegate void DisConnectDelegate(string id);
        /// <summary>
        /// 断开事件
        /// </summary>
        public event DisConnectDelegate? DisConnectEvent;

        /// <summary>
        /// 根据ID从管道池中释放一个管道
        /// </summary>
        private void DisposablePipeLine(string guid)
        {
            if (ServerPool.TryRemove(guid, out Pipeline? pipeline))
            {
                pipeline.Dispose();
                ThreadPool.QueueUserWorkItem(p => DisConnectEvent?.Invoke(guid));
                Console.WriteLine($"管道{guid},已经关闭");
            }
            else
            {
                Console.WriteLine($"未找到ID为{guid}的管道");
            }
        }
    }

    partial class Pipeline : IDisposable
    {
        readonly StreamReader StreamReader;
        readonly StreamWriter StreamWriter;
        readonly NamedPipeServerStream namedPipeServerStream;
        public bool IsConnected => namedPipeServerStream.IsConnected;
        public Pipeline(string PipeName, int MaxServer = 100)
        {
            namedPipeServerStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, MaxServer, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            StreamReader = new StreamReader(namedPipeServerStream, Encoding.UTF8, true, 1024 * 64);
            StreamWriter = new StreamWriter(namedPipeServerStream, Encoding.UTF8, 1024 * 64);
        }
        /// <summary>
        /// 等待连接
        /// </summary>
        public void WaitForConnection()
        {
            namedPipeServerStream.WaitForConnection();
        }
        /// <summary>
        /// 读取
        /// </summary>
        /// <returns></returns>
        public string? ReadLine()
        {
            return StreamReader.ReadLine();
        }
        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="massager"></param>
        public bool WriteLine(string? massager)
        {
            if (namedPipeServerStream.IsConnected)
            {
                StreamWriter.WriteLine(massager);
                StreamWriter.Flush();
                return true;
            }
            return false;
        }
        public void Dispose()
        {
            namedPipeServerStream.Close();
            namedPipeServerStream.Dispose();
        }
    }
}
