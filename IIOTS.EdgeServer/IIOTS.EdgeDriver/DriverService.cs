using IIOTS.Models;
using NetMQ.Sockets;
using IIOTS.Util;
using IIOTS.Interface;
using NetMQ;
using System.Diagnostics;
using IIOTS.EdgeDriver.Command;
using IIOTS.EdgeDriver.Manage;

namespace IIOTS.EdgeDriver
{
    /// <param name="_logger">
    /// 日志
    /// </param>
    /// <param name="_driverSignInInfo">
    /// 驱动登录信息
    /// </param>
    public class DriverService(
        ILogger<DriverService> _logger
        , ILoggerFactory loggerFactory
        , ProgressLoginInfo _driverSignInInfo
        , PublisherSocket publisher
        , SubscriberSocket subscriber) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            AutoResetEvent AutoResetEvent = new(false);
            subscriber.Subscribe($"{_driverSignInInfo.ClientId}");
            var handler = new Handler<IHandler>(_driverSignInInfo.ClientId, loggerFactory, publisher, AutoResetEvent);
            //等待1秒防止Publisher未连接
            await Task.Delay(1000);
            _logger.LogInformation($"发送登录信息【{_driverSignInInfo.ClientId}】");
            //发送登录信息
            publisher.LoginEdgeCore(_driverSignInInfo);
            _ = Task.Factory.StartNew(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (!AutoResetEvent.WaitOne(10000))
                    {
                        _logger.LogError($"未确认心跳正常退出进程【{_driverSignInInfo.ClientId}】");
                        Process.GetCurrentProcess().Kill();
                    }
                }
            }, TaskCreationOptions.LongRunning);
            _ = Task.Factory.StartNew(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    foreach (var driver in DriverManage.Drivers)
                    {
                        ThreadPool.QueueUserWorkItem(p =>
                        {
                            publisher.DriverStateChange(driver.Key, driver.Value.DriverState);
                        });
                    }
                    Task.Delay(5000).Wait();
                }
            }, TaskCreationOptions.LongRunning);
            subscriber.ReceiveReady += async (o, v) =>
            {  //接收主题名
                string topic = v.Socket.ReceiveFrameString();
                //接收信息
                string message = v.Socket.ReceiveFrameString();
                await Task.Run(() =>
                   {


                       try
                       {
                           //消息处理方法
                           HandlerResult result = handler.ExecuteHandler(topic, message);
                           switch (result.MsgType)
                           {
                               case Enums.MsgTypeEnum.Request:
                                   _logger.LogTrace($"{topic}回复请求【{result.Router}】【{result.Data}】");
                                   publisher.Send(result.Router, result.Data);
                                   break;
                               case Enums.MsgTypeEnum.Response:
                                   result.SetResponse();
                                   break;
                               case Enums.MsgTypeEnum.Execute:
                                   break;
                           }
                       }
                       catch (Exception e)
                       {
                           _logger.LogError($"处理失败，错误信息：【{e.Message}】");
                       }
                   });
            };
            new NetMQPoller() { subscriber }.Run();
        }
    }
}
