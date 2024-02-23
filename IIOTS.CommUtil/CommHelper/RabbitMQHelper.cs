using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace IIOTS.CommUtil
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
                UseBackgroundThreadsForIO = true,
                RequestedHeartbeat = TimeSpan.FromSeconds(6)
            };
            while (true)
            {
                try
                {
                    connection = factory.CreateConnection(connections);
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
        public void PubMessage(string Exchange, string data)
        {
            using (var channel = connection.CreateModel())
            {
                var body = Encoding.UTF8.GetBytes(data);
                channel.BasicPublish(Exchange, string.Empty, null, body);
            }
        }
        /// <summary>
        /// 创建消费者
        /// </summary>
        /// <param name="Exchange"></param>
        /// <param name="Queue"></param>
        /// <param name="receive"></param>
        public void CreationConsumer(string Exchange, string Queue, Action<string> receive)
        {
            var channel = connection.CreateModel();
            channel.QueueDeclare(Queue, true, false, false, null);
            channel.QueueBind(Queue, Exchange, "#");
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body.ToArray());
                    receive.Invoke(message);
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"消息队列接收出错{ex.Message}");
                }
            };
            channel.BasicConsume(Queue, false, consumer);

        }
    }
}
