using Confluent.Kafka; 
using IIOTS.Util;

namespace IIOTS.CommUtil
{
    public class KafkaHelper 
    {
        /// <summary>
        /// 连接地址
        /// </summary>
        private string? _BootstrapServers;
        /// <summary>
        /// 生产者
        /// </summary>
        IProducer<string, string> producer;
        public KafkaHelper(string? BootstrapServers)
        {
            _BootstrapServers = BootstrapServers;
            producer = new ProducerBuilder<string, string>(
                    new ProducerConfig
                    {
                        BootstrapServers = _BootstrapServers
                    }
                    )
                   .SetErrorHandler((_, e) =>
                   {
                   })
                   .Build();
        }
        /// <summary>
        /// 创建消费者
        /// </summary>
        /// <param name="GroupId"></param>
        /// <param name="Topics"></param>
        /// <param name="receive"></param>
        public void CreationConsumer(string GroupId, IEnumerable<string> Topics, Action<string, string> receive)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            IConsumer<Ignore, string> consumer = new ConsumerBuilder<Ignore, string>(new ConsumerConfig
            {
                GroupId = GroupId,
                BootstrapServers = _BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoOffsetStore = false
            }).SetErrorHandler((_, e) =>
            {

            }).Build();
            consumer.Subscribe(Topics);
            while (true)
            {
                var consumeResult = consumer.Consume(cts.Token);
                consumer.StoreOffset(consumeResult);
                receive.Invoke(GroupId, consumeResult.Message.Value);
            }
        }
        /// <summary>
        /// 生产数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Identity"></param>
        /// <param name="operateResult"></param>
        public void Send(string topic, string msg)
        {
            producer?.Produce(topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = msg
            }, r =>
            {
                if (r.Error.IsError)
                {

                }
            });
        }
        /// <summary>
        /// 生产数据
        /// </summary>
        /// <param name="Identity"></param>
        /// <param name="data"></param>
        public void Send<T>(string Identity, T data)
        {
            producer?.Produce(Identity, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = data.ToJson()
            }, r =>
            {
                if (r.Error.IsError)
                {

                }
            });
        }
    }
}
