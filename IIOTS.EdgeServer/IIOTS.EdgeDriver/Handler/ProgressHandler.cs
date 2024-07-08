using IIOTS.Driver;
using IIOTS.EdgeDriver.Command;
using IIOTS.EdgeDriver.Manage;
using IIOTS.Interface;
using IIOTS.Models;
using IIOTS.Util;
using NetMQ.Sockets;
using System.Reflection;

namespace IIOTS.EdgeDriver.Handler
{
    public class ProgressHandler(ILoggerFactory loggerFactory, PublisherSocket publisher) : IHandler
    {
        private readonly ILogger<ProgressHandler> logger = loggerFactory.CreateLogger<ProgressHandler>();

        private readonly TaskQueue taskQueue = new(TimeSpan.FromMilliseconds(500));
        /// <summary>
        /// 加载进程配置信息
        /// </summary>
        /// <param name="progressConfig"></param> 
        public void SetConfig(ProgressConfig progressConfig)
        {
            logger.LogInformation($"加载配置文件【{progressConfig.Name}】【{progressConfig.Description}】共需要启动【{progressConfig.EquConfigs.Count}】驱动");
            foreach (var equInfo in progressConfig.EquConfigs)
            {
                AddEqu(equInfo);
                Thread.Sleep(100);
            }
        }
        /// <summary>
        /// 增加设备
        /// </summary>
        /// <param name="equInfo"></param>
        /// <exception cref="NotImplementedException"></exception>
        public bool AddEqu(EquConfig equInfo)
        {
            try
            {
                //获取驱动类型
                string DLLName = equInfo.DriverType.ToString();
                var type = Assembly
                    .LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"IIOTS.Driver.{DLLName}.dll"))
                    .GetType($"IIOTS.Driver.{DLLName}");
                if (type == null)
                {
                    logger.LogError($"未找到驱动类型【{DLLName}】");
                    return false;
                }
                else
                {
                    DriverManage.Remove(equInfo.EQU);
                    //创建实例
                    if (Activator.CreateInstance(type, [equInfo.ConnectionString], null) is BaseDriver baseDriver)
                    {
                        //新增
                        DriverManage.Add(equInfo.EQU, baseDriver);
                        baseDriver.AddTags(equInfo.Tags.ChangeType<List<Tag>>());
                        baseDriver.AllTags.ForEach(p =>
                        {
                            p.ValueChangeEvent += (Tag tag) =>
                            {
                                publisher.ValueChange(equInfo.EQU, tag);
                            };
                        });
                        baseDriver.Start(equInfo.ScanRate);
                        logger.LogInformation($"启动【{equInfo.EQU}】驱动成功,驱动类型【{equInfo.DriverType}】连接字符串【{equInfo.ConnectionString}】");
                        return true;
                    }
                    else
                    {
                        logger.LogError($"初始化【{equInfo.EQU}】驱动【{DLLName}】失败");
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError($"增加驱动【{equInfo.EQU}】发生异常【{e.Message}】");
                return false;
            }
        }
        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="eques"></param> 
        public bool RemoveEqu(List<string> eques)
        {
            try
            {
                foreach (var equ in eques)
                {
                    if (!DriverManage.Remove(equ))
                    {
                        logger.LogError($"删除设备{equ}失败");
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                logger.LogError($"删除设备异常{e.Message}");
                return false;
            }

        }
        /// <summary>
        /// 启动设备
        /// </summary>
        /// <param name="eques"></param> 
        public bool StartEqu(List<string> eques)
        {
            try
            {
                foreach (var equ in eques)
                {
                    DriverManage.Start(equ);
                }
                return true;
            }
            catch (Exception e)
            {
                logger.LogError($"启动设备异常{e.Message}");
                return false;
            }
        }
        /// <summary>
        /// 停止设备
        /// </summary>
        /// <param name="eques"></param>
        public bool StopEqu(List<string> eques)
        {
            try
            {
                foreach (var equ in eques)
                {
                    DriverManage.Stop(equ);
                }
                return true;
            }
            catch (Exception e)
            {
                logger.LogError($"停止设备异常{e.Message}");
                return false;
            }
        }
    }
}
