using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace IIOTS.Util
{
    public class RabbitMQHelper
    {
        readonly IConnection connection;
        public RabbitMQHelper(string? userName, string? password, int port, string[]? connections)
        {
            var factory = new ConnectionFactory()
            {
                UserName = userName,
                Password = password,
                Port = port,
                AutomaticRecoveryEnabled = true,
                HandshakeContinuationTimeout = TimeSpan.FromSeconds(1),
                ContinuationTimeout = TimeSpan.FromSeconds(2),
                RequestedConnectionTimeout = TimeSpan.FromSeconds(3),
                SocketReadTimeout = TimeSpan.FromSeconds(3),
                SocketWriteTimeout = TimeSpan.FromSeconds(3),
                NetworkRecoveryInterval = TimeSpan.FromSeconds(0.5),
                RequestedHeartbeat = TimeSpan.FromSeconds(6)
            };
            while (true)
            {
                try
                {
                    connection = factory.CreateConnectionAsync(connections).Result;
                    Console.WriteLine("连接消息队列服务器成功");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"连接消息队列服务器出错{ex.Message}");
                    Thread.Sleep(1000);
                }
            }
        }
        /// <summary>
        /// 生产信息
        /// </summary>
        /// <param name="Exchange"></param>
        /// <param name="data"></param>
        public async void PubMessage(string Exchange, string data)
        {
            using (var channel = await connection.CreateChannelAsync())
            {
                var body = Encoding.UTF8.GetBytes(data);
                await channel.BasicPublishAsync(
                    Exchange, string.Empty, true, new BasicProperties
                    {
                        ContentType = "application/json",
                        Persistent = true
                    }, body);
            }
        }
        /// <summary>
        /// 创建消费者
        /// </summary>
        /// <param name="Exchange"></param>
        /// <param name="Queue"></param>
        /// <param name="receive"></param>
        public async void CreationConsumer(string Exchange, string Queue, Action<string> receive)
        {
            var channel = await connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(Queue, true, false, false, null);
            await channel.QueueBindAsync(Queue, Exchange, "#");
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body.ToArray());
                    receive.Invoke(message);
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"消息队列接收出错{ex.Message}");
                }
            };
            await channel.BasicConsumeAsync(Queue, false, consumer);

        }
    }
}
