using IIOTS.Models;
using NetMQ.Sockets;
using IIOTS.Util;

namespace IIOTS.EdgeDriver.Command
{
    public static class ChangeEvent
    {
        /// <summary>
        /// 点位变化事件
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="progressLogin"></param>
        public static void ValueChange(this PublisherSocket publisher, string equ, Tag tag)
        {
            publisher.Send(string.Join("/", ["ValueChange", equ, tag.TagName]), tag);
        }
        /// <summary>
        /// 设备状态变化事件
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="progressLogin"></param>
        public static void DriverStateChange(this PublisherSocket publisher, string equ, bool DriverState)
        {
            publisher.Send(string.Join("/", ["DriverStateChange", equ]), DriverState);
        }
    }
}
