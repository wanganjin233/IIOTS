using IIOTS.Driver;
using System.Collections.Concurrent;

namespace IIOTS.EdgeDriver.Manage
{
    public class DriverManage
    {
        /// <summary>
        /// 设备对应配置和管道服务
        /// </summary>
        private static readonly ConcurrentDictionary<string, BaseDriver> drivers = new();
        public static ConcurrentDictionary<string, BaseDriver> Drivers => drivers;
        /// <summary>
        /// 获取驱动
        /// </summary>
        /// <param name="equ"></param>
        /// <param name="driver"></param>
        public static BaseDriver? Get(string equ)
        {
            drivers.TryGetValue(equ, out BaseDriver? baseDriver);
            return baseDriver;
        }
        /// <summary>
        /// 添加设备
        /// </summary>
        /// <param name="equ"></param>
        /// <param name="driver"></param>
        public static void Add(string equ, BaseDriver driver)
        {
            drivers[equ] = driver;
        }
        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="equ"></param>
        /// <param name="driver"></param>
        public static bool Remove(string equ)
        {
            bool rv = drivers.TryRemove(equ, out BaseDriver? driver);
            driver?.Dispose();
            return rv;
        }
        /// <summary>
        /// 停止设备
        /// </summary>
        /// <param name="equ"></param>
        /// <param name="driver"></param>
        public static void Stop(string equ)
        {
            if (drivers.TryGetValue(equ, out BaseDriver? value))
            {
                value.Stop();
            }
        }
        /// <summary>
        /// 启动设备
        /// </summary>
        /// <param name="equ"></param>
        /// <param name="cycle"></param>
        public static void Start(string equ, int cycle = 100)
        {
            if (drivers.TryGetValue(equ, out BaseDriver? value))
            {
                value.Start(cycle);
            }
        }

    }
}
