using IIOTS.Util;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System.Collections.Concurrent;

namespace IMEC.WebRMS.Services
{
    public interface IMqttClientService : IDisposable
    {
        /// <summary>
        /// 连接状态
        /// </summary>
        bool IsConnected { get; }
        bool Publish(string topic, string? payload, bool retain);
        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="mqttTopicFilter"></param>
        void Subscribe(MqttTopicFilter mqttTopicFilter);
        /// <summary>
        /// 卸载主题
        /// </summary>
        /// <param name="mqttTopic"></param>
        void UnSubscribe(string mqttTopic);
        /// <summary>
        /// 接收事件
        /// </summary>
        event Action<string, string>? ApplicationMessageReceived;
    }
    public class MqttClientService : IMqttClientService
    {
        readonly ConcurrentDictionary<string, MqttTopicFilter> mqttTopicFilters = new();
        readonly IMqttClient _MqttClient;
        public MqttClientService()
        {
            string? MQTTIP = AppConfigurationHelper.Configuration.GetSection("MQTT:IP").Get<string>();
            int? MQTTPort = AppConfigurationHelper.Configuration.GetSection("MQTT:Port").Get<int>();
            if (MQTTIP.IsNullOrEmpty() || MQTTIP.IsNullOrEmpty())
            {
                throw new Exception("Mqtt连接配置错误");
            }
            //创建MQTT客户端
            _MqttClient = new MqttFactory().CreateMqttClient();
            //断开重联
            _MqttClient.DisconnectedAsync += a => _MqttClient.ReconnectAsync();
            _MqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
            _MqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
            AsyncHelper.RunSync(() => _MqttClient.ConnectAsync(new MqttClientOptionsBuilder()
                .WithTcpServer(MQTTIP, MQTTPort)
                .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                .Build()));
        }

        private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            return Task.Run(() =>
            {
                string? message = arg.ApplicationMessage.ConvertPayloadToString();
                if (!string.IsNullOrEmpty(message))
                {
                    ApplicationMessageReceived?.Invoke(arg.ApplicationMessage.Topic, message);
                }
            });
        }

        public bool Publish(string topic, string? payload, bool retain)
        {
            return AsyncHelper.RunSync(() => _MqttClient.PublishStringAsync(topic
                                               , payload
                                               , MqttQualityOfServiceLevel.ExactlyOnce
                                               , retain)).IsSuccess;
        }

        /// <summary>
        /// 连接事件
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private async Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            foreach (var mqttTopicFilter in mqttTopicFilters.Values)
            {
                await _MqttClient.SubscribeAsync(mqttTopicFilter);
            }
        }
        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="mqttTopicFilter"></param>
        public void Subscribe(MqttTopicFilter mqttTopicFilter)
        {
            if (mqttTopicFilters.TryAdd(mqttTopicFilter.Topic, mqttTopicFilter))
            {
                AsyncHelper.RunSync(() => _MqttClient.SubscribeAsync(mqttTopicFilter));
            }
        }
        /// <summary>
        /// 卸载主题
        /// </summary>
        /// <param name="mqttTopic"></param>
        public void UnSubscribe(string mqttTopic)
        {
            if (mqttTopicFilters.TryRemove(mqttTopic, out _))
            {
                AsyncHelper.RunSync(() => _MqttClient.UnsubscribeAsync(mqttTopic));
            }
        }

        public void Dispose()
        {
            _MqttClient.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Mqtt客户端连接状态
        /// </summary> 
        public bool IsConnected => _MqttClient.IsConnected;
        /// <summary>
        /// 接收事件
        /// </summary>
        public event Action<string, string>? ApplicationMessageReceived;
    }
}
