using IIOTS.EdgeCore.Manage;
using IIOTS.Models;
using IIOTS.Util;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace IIOTS.EdgeCore.Service
{
    public class ProgressService : BackgroundService
    {
        private readonly ILogger<ProgressService> _logger;
        private readonly ProgressManage _progressManage;

        public ProgressService(ILogger<ProgressService> logger, ProgressManage progressManage)
        {
            _logger = logger;
            _progressManage = progressManage;
            //判断是否为本地运行模式
            if (AppConfigurationHelper.Configuration.GetSection("LocalMode").Get<bool>())
            {
                _logger.LogInformation("本地运行模式");
                #region 载入设备配置 
                //获取全部配置文件
                string[] enableConfigPaths = Directory.GetFiles(Config.EnableConfigPath);
                foreach (var enableConfigPath in enableConfigPaths)
                {
                    //读取配置
                    ProgressConfig? progressConfig = File.ReadAllText(enableConfigPath).ToObject<ProgressConfig>();
                    if (progressConfig != null)
                    {
                        //去除重复启动进程配置
                        progressConfig.Operations = progressConfig.Operations
                        .Where((x, i) => progressConfig.Operations.FindIndex(s => s == x) == i)
                        .ToList();
                        foreach (var Operation in progressConfig.Operations)
                        {
                            //添加到进程管理
                            _progressManage.AddDriverLoginInfo(new ProgressLoginInfo
                            {
                                Name = progressConfig.Name,
                                ClientType = Operation
                            });
                        }
                    }
                }
                #endregion
            }
            //联网模式
            else
            {
                _logger.LogInformation("联网集群模式");
            }
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int cycleTime = 15000;
            while (!stoppingToken.IsCancellationRequested)
            {
                #region 检测进程状态   
                await Task.Delay(cycleTime, stoppingToken);
                //获取没有运行的进程
                var needRuns = _progressManage.ProgressNotRunList();
                //启动进程
                foreach (var needRun in needRuns)
                {
                    //Windows平台下运行
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        _logger.LogInformation($"正在启动【{needRun.Name}】的【{needRun.ClientType}】进程，发布端口{Config.PublisherPort} 订阅端口{Config.SubscriberPort}");
                        ProcessStartInfo _processStartInfo = new()
                        {
                            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                            FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{needRun.ClientType}.exe"),
                            Arguments = $"127.0.0.1:{Config.PublisherPort} 127.0.0.1:{Config.SubscriberPort} {needRun.Name} {new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()}",
                            CreateNoWindow = false
                        };
                        Process.Start(_processStartInfo);
                    }
                    //Linux平台下运行
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        _logger.LogInformation($"正在启动【{needRun.Name}】的【{needRun.ClientType}】进程，发布端口{Config.PublisherPort} 订阅端口{Config.SubscriberPort}");
                        ProcessStartInfo _processStartInfo = new()
                        {
                            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                            FileName = "dotnet",
                            Arguments = $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{needRun.ClientType}.dll")} 127.0.0.1:{Config.PublisherPort} 127.0.0.1:{Config.SubscriberPort} {needRun.Name} {new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()}",
                            CreateNoWindow = false
                        };
                        Process.Start(_processStartInfo);
                    }
                    //其他平台下运行
                    else
                    {
                        throw new NotImplementedException("暂不支持此平台运行");
                    }
                    try
                    {

                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"无法启动【{needRun.ClientType}】【{e.Message}】");
                    }
                }
                #endregion 
            }
        }
    }
}
