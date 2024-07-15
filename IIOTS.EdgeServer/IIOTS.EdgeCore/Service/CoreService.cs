using IIOTS.Interface;
using IIOTS.Util;
using MQTTnet.Client;
using MQTTnet;
using NetMQ;
using NetMQ.Sockets;
using MQTTnet.Protocol;
using IIOTS.EdgeCore.Manage;
using IIOTS.Models;
using System.Text;
using MQTTnet.Server;
using IIOTS.EdgeCore.Command;

namespace IIOTS.EdgeCore.Service
{
    public class CoreService : BackgroundService
    {
        /// <summary>
        /// 创建MQTT客户端 
        /// </summary>
        private readonly IMqttClient mqttClient = new MqttFactory().CreateMqttClient();
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// 日志工厂
        /// </summary>
        private readonly ILoggerFactory _loggerFactory;
        /// <summary>
        /// 发布者
        /// </summary>
        private readonly PublisherSocket publisher;
        /// <summary>
        /// 订阅者
        /// </summary>
        private readonly SubscriberSocket subscriber;
        private readonly ProgressManage _progressManage;
        private readonly MQTTConcat MqttConcat;
        /// <summary>
        /// 消息处理方法
        /// </summary>
        private readonly Handler<IHandler> handler;
        public CoreService(ILogger<CoreService> logger, ILoggerFactory loggerFactory, ProgressManage progressManage, MQTTConcat Concat)
        {
            MqttConcat = Concat;
            _logger = logger;
            _loggerFactory = loggerFactory;
            _progressManage = progressManage;
            CreationBroker();
            publisher = new PublisherSocket($">tcp://127.0.0.1:{Config.PublisherPort}");
            subscriber = new SubscriberSocket($">tcp://127.0.0.1:{Config.SubscriberPort}");
            Task.Delay(1000).Wait();
            _logger.LogInformation($"ZeroMQ创建订阅者,开始订阅【{Config.Identifier}】主题");
            subscriber.Subscribe(Config.Identifier, Encoding.UTF8);
            subscriber.Subscribe("ValueChange", Encoding.UTF8);
            subscriber.Subscribe("DriverStateChange", Encoding.UTF8);
            handler = new Handler<IHandler>(Config.Identifier, _loggerFactory, publisher, mqttClient, progressManage);
        }
        /// <summary>
        /// 创建Broker实现多对多通讯
        /// </summary>
        /// <returns></returns>
        private void CreationBroker()
        {
            Task.Factory.StartNew(() =>
            {
                using var xsubSocket = new XSubscriberSocket($"@tcp://127.0.0.1:{Config.PublisherPort}");
                using var xpubSocket = new XPublisherSocket($"@tcp://127.0.0.1:{Config.SubscriberPort}");
                var proxy = new Proxy(xsubSocket, xpubSocket);
                _logger.LogInformation($"创建Broker成功【XSubscriberPort】端口号为【{Config.PublisherPort}】【XPublisherPort】端口号为【{Config.SubscriberPort}】");
                proxy.Start();
            }, TaskCreationOptions.LongRunning);
        }
        /// <summary>
        /// 连接MQTT服务
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ConnectMqtt(CancellationToken cancellationToken = default)
        {
            //断开重联
            mqttClient.DisconnectedAsync += a => mqttClient.ReconnectAsync(cancellationToken);
            //连接事件
            mqttClient.ConnectedAsync += async (e) =>
            {
                string topic = $"{Config.Identifier}/{_progressManage.EdgeLoginInfo.EdgeID}/#";
                _logger.LogInformation($"MQTT开始订阅【{topic}】主题");
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .Build());
                _logger.LogInformation($"MQTT开始订阅【EdgeCore/all/Equ/WriteTag】主题");
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                  .WithTopic("EdgeCore/all/Equ/WriteTag")
                  .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                  .Build());
                //发送上线信息
                _progressManage.UpdateDriverState(true);
            };
            //绑定MQTT接收消息事件
            mqttClient.ApplicationMessageReceivedAsync += async (e) =>
            {
                await Task.Run(() =>
                {
                    string? message = e.ApplicationMessage.ConvertPayloadToString();
                    if (e.ApplicationMessage.Topic == "EdgeCore/all/Equ/WriteTag")
                    {
                        if (message.TryToObject(out Operate<Tag>? tag))
                        {
                            foreach (var progressLogin in _progressManage.GetAllProgress())
                            {
                                //获取进程下所有驱动
                                List<string>? driverNames = publisher.GetDrivers(progressLogin.ClientId);
                                if (driverNames?.Any(p => tag.Id == p) ?? false)
                                {
                                    publisher.WriteTag(progressLogin.ClientId, tag);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(message))
                            {
                                List<string> routers = [.. e.ApplicationMessage.Topic.Split("/")];
                                routers.RemoveAt(1);
                                handler.ExecuteHandler(string.Join("/", routers), message);
                            }
                        }
                        catch (Exception e)
                        {

                            _logger.LogError($"处理失败，错误信息：【{e.Message}】");
                        }
                    }

                });
            };
            _progressManage.UpdateDriverState(false);
            string willMessage = _progressManage.EdgeLoginInfo.ToJson();
            //连接MQTT服务
            await mqttClient.ConnectAsync(new MqttClientOptionsBuilder()
               .WithTcpServer(MqttConcat.IP, MqttConcat.Port)
               .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
               .WithWillTopic($"EdgeLoginInfo/{_progressManage.EdgeLoginInfo.EdgeID}")
               .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
               .WithWillRetain(true)
               .WithWillPayload(willMessage)
               .Build(), cancellationToken);
        }

        /// <summary>
        /// 定时检测进程状态
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task CheckProgressActive(CancellationToken cancellationToken = default)
        {
            await Task.Factory.StartNew(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Task.Delay(5000).Wait();
                    foreach (var driverLoginInfo in _progressManage.GetAllProgress())
                    {
                        string clientId = driverLoginInfo.ClientId;
                        ThreadPool.QueueUserWorkItem(p =>
                        {
                            try
                            {
                                _logger.LogTrace($"检测客户端【{clientId}】心跳");
                                long workerId = publisher.CheckActive(clientId);
                                _logger.LogTrace($"接收到客户端【{workerId}】心跳");
                                if (workerId != 0)
                                {
                                    _progressManage.RefreshHeartBeat(clientId);
                                    publisher.ConfirmActive(clientId, workerId);
                                }
                            }
                            catch (Exception)
                            {
                                _logger.LogError($"检测客户端超时");

                            }
                        });
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConnectMqtt(stoppingToken).Wait();

            _ = Task.Factory.StartNew(async () =>
              {
                  while (!stoppingToken.IsCancellationRequested)
                  {
                      try
                      {
                          EdgeLoginInfo edgeLogin = _progressManage.EdgeLoginInfo;
                          lock (mqttClient)
                          {
                              mqttClient.PublishAsync(new MqttApplicationMessage()
                              {
                                  Topic = $"EdgeLoginInfo/{_progressManage.EdgeLoginInfo.EdgeID}",
                                  PayloadSegment = Encoding.UTF8.GetBytes(new EdgeLoginInfo()
                                  {
                                      EdgeID = edgeLogin.EdgeID,
                                      StartTime = edgeLogin.StartTime,
                                      State = edgeLogin.State,
                                      ProgressLoginInfos = _progressManage.ProgressRunList()
                                      .Select(p =>
                                      new ProgressLoginInfo()
                                      {
                                          Name = p.Name,
                                          ClientType = p.ClientType,
                                          HeartbeatTime = p.HeartbeatTime,
                                          StartTime = p.StartTime,
                                          progressConfig = p.progressConfig == null ? null : new ProgressConfig()
                                          {
                                              Name = p.progressConfig.Name,
                                              Description = p.progressConfig.Description,
                                              Operations = p.progressConfig.Operations,
                                              EquConfigs = p.progressConfig.EquConfigs.Select(p => new EquConfig()
                                              {
                                                  ScanRate = p.ScanRate,
                                                  ConnectionString = p.ConnectionString,
                                                  Description = p.Description,
                                                  DriverType = p.DriverType,
                                                  Enable = p.Enable,
                                                  EQU = p.EQU,
                                                  TagConfigPath = p.TagConfigPath,
                                                  Tags = []
                                              }).ToList()
                                          }
                                      })
                                      .ToList(),
                                  }.ToJson()),
                                  QualityOfServiceLevel = MqttQualityOfServiceLevel.ExactlyOnce,
                                  Retain = true
                              }, stoppingToken).Wait();
                          }
                      }
                      finally
                      {
                          await Task.Delay(10000);
                      }
                  }
              }, TaskCreationOptions.LongRunning);
            subscriber.ReceiveReady += async (o, v) =>
            {
                //接收主题名
                string topic = v.Socket.ReceiveFrameString();
                //接收信息
                string message = v.Socket.ReceiveFrameString();

                try
                {
                    //点位变化主题发送至MQTT
                    if (topic.StartsWith("ValueChange"))
                    {
                        await mqttClient.PublishAsync(new MqttApplicationMessage()
                        {
                            Topic = topic,
                            PayloadSegment = Encoding.UTF8.GetBytes(message),
                            QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                            Retain = true
                        });
                    }
                    else if (topic.StartsWith("DriverStateChange"))
                    {
                        await mqttClient.PublishAsync(new MqttApplicationMessage()
                        {
                            Topic = topic,
                            PayloadSegment = Encoding.UTF8.GetBytes(message),
                            QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                            Retain = true,
                            MessageExpiryInterval = 10
                        });
                    }
                    else
                    {
                        await Task.Run(() =>
                          {
                              //消息处理方法
                              HandlerResult result = handler.ExecuteHandler(topic, message);
                              switch (result.MsgType)
                              {
                                  case Enums.MsgTypeEnum.Request:
                                      publisher.Send(result.Router, result.Data);
                                      break;
                                  case Enums.MsgTypeEnum.Response:
                                      result.SetResponse();
                                      break;
                                  case Enums.MsgTypeEnum.Execute:
                                      break;
                              }
                          });
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"主题【{topic}】内容【{message}】处理失败，错误信息：【{e.Message}】");
                }
            };
            new NetMQPoller() { subscriber }.RunAsync();
            await CheckProgressActive(stoppingToken);
        }
    }
}
