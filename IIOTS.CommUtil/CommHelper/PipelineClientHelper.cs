using IIOTS.Communication;
using IIOTS.Models;
using IIOTS.Util;

namespace IIOTS.CommUtil
{
    public class PipelineClientHelper
    {
        /// <summary>
        /// 管道客户端
        /// </summary>
        private readonly PipelineClient client;
        /// <summary>
        /// 缓存操作
        /// </summary>
        private readonly FileCache cacheHelper;
        /// <summary>
        /// 管道名称
        /// </summary> 
        public string PipelineName => client.PipelineName;

        public string Identifier;
        /// <summary>
        /// 初始化连接
        /// </summary>
        public PipelineClientHelper(string pipeName, string Identifier)
        {
            this.Identifier = Identifier;
            cacheHelper = new(pipeName);
            client = new PipelineClient(pipeName, $"{Identifier};{Environment.ProcessId}");
            client.Connect();
            client.ReceiveEvent += Client_ReceiveEvent;
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Task.Delay(3000).Wait();
                    client.Send("0");
                }
            },TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    string message = cacheHelper.Dequeue();
                    if (client.Send(message))
                    {
                        cacheHelper.ACK();
                    }
                    else
                    {
                        Task.Delay(5000).Wait();
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
        /// <summary>
        /// 接收数据处理
        /// </summary>
        /// <param name="pipelineClient"></param>
        /// <param name="data"></param>
        private void Client_ReceiveEvent(PipelineClient pipelineClient, string data)
        {
            ReceiveEvent?.Invoke(this, data);
        }
        /// <summary>
        /// 接收委托
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="data">数据</param>
        public delegate void ReceiveDelegate(PipelineClientHelper pipelineClient, string data);
        /// <summary>
        /// 接收事件
        /// </summary>
        public event ReceiveDelegate? ReceiveEvent;
        /// <summary>
        /// 发送到服务端
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Identity"></param>
        /// <param name="operateResult"></param>
        public void Send<T>(OperateResult<T> operateResult)
        {
            var data = operateResult.ToJson();
            client.Send(data, (msg) =>
            {
                if (msg != null)
                {
                    cacheHelper.Enqueue(data);
                }
            });
        }
        /// <summary>
        /// 回复信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operateResult"></param>
        public void Reply<T>(OperateResult<T> operateResult)
        {
            operateResult.ReceiverIdentity = operateResult.SenderIdentity;
            operateResult.SenderIdentity = null;
            Send(operateResult);
        }

    }
}
