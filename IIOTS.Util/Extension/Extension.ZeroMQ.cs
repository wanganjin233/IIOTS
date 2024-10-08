﻿using IIOTS.Models;
using NetMQ;
using NetMQ.Sockets;

namespace IIOTS.Util
{
    public sealed class ResetEventData(bool initialState, EventResetMode mode = EventResetMode.AutoReset) : EventWaitHandle(initialState, mode)
    {
        public string? Data { get; set; }
    }
    /// <summary>
    /// 拓展类
    /// </summary>
    public static partial class Extension
    {

        /// <summary>
        /// 接收回应数据的阻塞词典
        /// </summary>
        private static readonly UsingLock<Dictionary<string, ResetEventData>> zeroAutoResetEvent = new([]);

        /// <summary>
        /// 释放接收阻塞
        /// </summary>
        /// <param name="handlerResult"></param>
        public static void SetResponse(this HandlerResult handlerResult)
        {
            using (zeroAutoResetEvent.Read())
            {
                if (handlerResult.MsgCode != null
                    && zeroAutoResetEvent.Data.TryGetValue(handlerResult.MsgCode, out ResetEventData? resetEventData))
                {
                    if (handlerResult.Data is string data)
                    {
                        resetEventData.Data = data;
                    }
                    else
                    {
                        resetEventData.Data = handlerResult.Data.ToJson();
                    }
                    resetEventData.Set();
                }
            }

        }
        /// <summary>
        /// 发布入队
        /// </summary>
        /// <param name="publisherSocket"></param>
        /// <param name="topic"></param>
        /// <param name="obj"></param>
        public static void Send(this PublisherSocket publisherSocket,
            string? topic,
            object? obj)
        {
            if (topic != null)
            {
                lock (publisherSocket)
                {
                    publisherSocket
                        .SendMoreFrame(topic)
                        .SendFrame(obj?.ToJson() ?? "");
                }
            }
        }
        /// <summary>
        /// 问答
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="publisherSocket"></param>
        /// <param name="topic"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T? Reply<T>(this PublisherSocket publisherSocket,
            string topic,
            object? obj = null)
        {
            string id = IdHelper.GetId();
            var autoReset = new ResetEventData(false);
            using (zeroAutoResetEvent.Write())
            {
                zeroAutoResetEvent.Data.TryAdd(id, autoReset);
            }
            publisherSocket.Send($"{topic}/{Config.Identifier}/Request/{id}", obj);
            T? result = default;
            bool waitResult = autoReset.WaitOne(3000);
            if (waitResult)
            {
                result = autoReset.Data.ToObject<T>();
            }
            using (zeroAutoResetEvent.Write())
            {
                zeroAutoResetEvent.Data.Remove(id, out _);
            }
            autoReset.Dispose();
            if (!waitResult)
            {
                //throw new Exception("请求超时");
            }
            return result;

        }


    }
}
