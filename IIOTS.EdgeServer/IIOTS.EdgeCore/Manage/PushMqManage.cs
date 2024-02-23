using IIOTS.Models;
using IIOTS.Util;
using System.Collections.Concurrent;

namespace IIOTS.EdgeCore.Manage
{
    public class PushMqManage
    {
        /// <summary> 
        /// 订阅队列对应推送客户端
        /// </summary>
        private static readonly ConcurrentDictionary<Tag, List<string>> SubMqQueues = new();

        /// <summary>
        /// 订阅队列
        /// </summary>
        /// <param name="socketClientInfo"></param>
        /// <param name="socketServer"></param>
        /// <param name="rabbitMQHelper"></param>
        /// <param name="routes"></param>
        /// <returns></returns>
     // public static List<Tag> SubMqQueue(
     //     SocketClientInfo socketClientInfo,
     //     SocketServerHelper socketServer,
     //     RabbitMQHelper rabbitMQHelper,
     //     List<string> routes)
     // {
     //     List<Tag> queues = new List<Tag>();
     //     foreach (var route in routes)
     //     {
     //         //根据名称找到Tag点
     //         var tag = SubMqQueues.Keys.FirstOrDefault(p => p.TagName == route);
     //         bool subFlag = false;
     //         //判断Tag点不存在，获取需要推送的客户端
     //         if (tag == null)
     //         {
     //             tag = new Tag() { TagName = route };
     //             SubMqQueues.TryAdd(tag, new List<string>() { socketClientInfo.ClientId });
     //             subFlag = true;
     //         }
     //         else if (SubMqQueues.TryGetValue(tag, out List<string>? clientIds)) //如果Tag点存在
     //         {
     //             subFlag = !clientIds.Contains(socketClientInfo.ClientId);
     //         }
     //         if (subFlag)
     //         {
     //             string[] routeSplit = route.Split('/');
     //             string exchange = routeSplit[0];
     //             string queue = routeSplit[1];
     //             //订阅mq主题
     //             rabbitMQHelper.CreationConsumer(exchange, queue, (s) =>
     //             {
     //                 tag.Value = s;
     //                 BaseCommands.TagPush(socketClientInfo.ClientId, socketServer, tag);
     //             });
     //         }
     //         queues.Add(tag);
     //     }
     //     return queues;
     // }

        public static bool PubMqQueue(RabbitMQHelper rabbitMQHelper, string data)
        {
            try
            {
                int index = data.IndexOf(":");
                rabbitMQHelper.PubMessage(data[..index], data[(index + 1)..]);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}