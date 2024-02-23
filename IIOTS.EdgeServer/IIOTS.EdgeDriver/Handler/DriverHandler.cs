using IIOTS.EdgeDriver.Manage;
using IIOTS.Interface;
using IIOTS.Models;
using NetMQ.Sockets;

namespace IIOTS.EdgeDriver.Handler
{
    public class DriverHandler(ILoggerFactory loggerFactory, PublisherSocket publisher) : IHandler
    {
        private readonly ILogger<DriverHandler> logger = loggerFactory.CreateLogger<DriverHandler>();
        /// <summary>
        /// 获取所有驱动
        /// </summary>
        /// <returns></returns>
        public List<string> GetDrivers()
        {
            return DriverManage.Drivers.Keys.ToList();
        }
        /// <summary>
        /// 写入Tag点位
        /// </summary>
        /// <param name="Tags"></param>
        /// <returns></returns>
        public bool WriteTag(Operate<Tag> tag)
        {
            var baseDriver = DriverManage.Get(tag.Id);
            if (baseDriver != null)
            {
                baseDriver.Write(tag.Content.TagName, tag.Content.Value);
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 添加点位
        /// </summary>
        /// <param name="OperateTags"></param>
        /// <returns></returns>
        public bool AddTags(Operate<List<Tag>> OperateTags)
        {
            var baseDriver = DriverManage.Get(OperateTags.Id);
            if (baseDriver != null)
            {
                baseDriver.AddTags(OperateTags.Content);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
