using IIOTS.Communication;
using IIOTS.Models;
using IIOTS.Util;
using System.Collections.Concurrent;

namespace IIOTS.CommUtil
{
    public class PipelineServerHelper
    {
        /// <summary>
        /// socket客户端
        /// </summary>
        private readonly PipelineServer server;

        public string PipeName => server.PipeName;

        public List<string> PipelineIds => server.PipelineIds;

        private readonly ConcurrentDictionary<string, FileCache> cacheHelpers = new();
        /// <summary>
        /// 初始化连接
        /// </summary>
        public PipelineServerHelper(string pipeName)
        {
            server = new PipelineServer(pipeName);
            server.ConnectEvent += Server_ConnectEvent;
            server.ReceiveEvent += Server_ReceiveEvent; ;
        }

        private void Server_ReceiveEvent(PipelineServer pipelineServer, string id, string data)
        {
            ReceiveEvent?.Invoke(this, id, data);
        }
        /// <summary>
        /// 接收委托
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="data">数据</param>
        public delegate void ReceiveDelegate(PipelineServerHelper pipelineServer, string id, string data);
        /// <summary>
        /// 接收事件
        /// </summary>
        public event ReceiveDelegate? ReceiveEvent;


        /// <summary>
        /// 连接事件
        /// </summary>
        /// <param name="id"></param>
        private void Server_ConnectEvent(string id)
        {
            FileCache cacheHelper = cacheHelpers.GetOrAdd(PipeName + id.Split(";")[0], (str) =>
            {
                FileCache cacheHelper = new(PipeName + id.Split(";")[0]);
                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        string message = cacheHelper.Dequeue();
                        if (server.Send(id, message))
                        {
                            cacheHelper.ACK();
                        }
                        else
                        {
                            Task.Delay(5000).Wait();
                        }
                    }
                }, TaskCreationOptions.LongRunning);
                return cacheHelper;
            });
        }
        /// <summary>
        /// 发送到指定客户端
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="Identifier"></param>
        /// <param name="operateResult"></param>
        /// <param name="Cache"></param>
        public void Send<T>(string id, OperateResult<T> operateResult, bool Cache = true)
        {
            FileCache cacheHelper = cacheHelpers.GetOrAdd(PipeName + id.Split(";")[0], new FileCache(id));
            string data = operateResult.ToJson();
            server.Send(id, data, (guid, msg) =>
            {
                if (Cache && msg != null)
                {
                    cacheHelper.Enqueue(data);
                }
            });
        }
        /// <summary>
        /// 回复
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="operateResult"></param>
        public void Reply<T>(string id, OperateResult<T> operateResult)
        {
            operateResult.ReceiverIdentity = operateResult.SenderIdentity;
            operateResult.SenderIdentity = null;
            server.Send(id, operateResult.ToJson());
        }
    }
}
