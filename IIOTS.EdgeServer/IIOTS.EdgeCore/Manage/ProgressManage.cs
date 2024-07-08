using IIOTS.Models;
using IIOTS.Util;

namespace IIOTS.EdgeCore.Manage
{
    public class ProgressManage
    {
        public ProgressManage(EdgeLoginInfo edgeLogin)
        {
            using (_EdgeLoginInfo.Write())
            {
                _EdgeLoginInfo.Data = edgeLogin;
            }
        }
        /// <summary>
        /// 节点信息
        /// </summary>
        private readonly UsingLock<EdgeLoginInfo> _EdgeLoginInfo = new();
        /// <summary>
        /// 进程心跳超时时间
        /// </summary>
        private readonly TimeSpan timeSpan = TimeSpan.FromMilliseconds(20000);
        /// <summary>
        /// 节点登录信息
        /// </summary>
        public EdgeLoginInfo EdgeLoginInfo
        {
            get
            {
                using (_EdgeLoginInfo.Read())
                {
                    return _EdgeLoginInfo.Data ?? new();
                }
            }
        }

        /// <summary>
        /// 更新驱动状态
        /// </summary>
        /// <param name="state"></param>
        public void UpdateDriverState(bool state)
        {
            using (_EdgeLoginInfo.Write())
            {
                EdgeLoginInfo.State = state;
            }
        }

        /// <summary>
        /// 获取全部进程
        /// </summary> 
        public List<ProgressLoginInfo> GetAllProgress()
        {
            using (_EdgeLoginInfo.Read())
            {
                return _EdgeLoginInfo.Data?.ProgressLoginInfos ?? [];
            }
        }
        /// <summary>
        /// 更新心跳
        /// </summary>
        /// <param name="driverLoginInfo"></param>
        /// <returns></returns>
        public void RefreshHeartBeat(string clientId)
        {
            using (_EdgeLoginInfo.Write())
            {
                var ProgressLoginInfo = _EdgeLoginInfo.Data?.ProgressLoginInfos.FirstOrDefault(p => p.ClientId == clientId);
                if (ProgressLoginInfo != null)
                {
                    ProgressLoginInfo.HeartbeatTime = DateTime.Now;
                }
            }
        }
        /// <summary>
        /// 添加设备配置信息
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="equConfig"></param>
        /// <returns></returns>
        public bool AddEquConfig(string clientId, EquConfig equConfig)
        {
            using (_EdgeLoginInfo.Write())
            {
                ProgressLoginInfo? progressLoginInfo = _EdgeLoginInfo.Data?.
                ProgressLoginInfos.
                FirstOrDefault(p => p.ClientId == clientId);
                if (progressLoginInfo == null)
                {
                    string[] clientIdSplit = clientId.Split("_");
                    _EdgeLoginInfo.Data?.ProgressLoginInfos.Add(new ProgressLoginInfo
                    {
                        Name = clientIdSplit[0],
                        ClientType = clientIdSplit[1],
                        progressConfig = new ProgressConfig
                        {
                            Name = clientIdSplit[0],
                            Operations = ["IIOTS.EdgeDriver"],
                            EquConfigs = [equConfig]
                        }
                    });
                }
                else if (progressLoginInfo.progressConfig == null)
                {
                    string[] clientIdSplit = clientId.Split("_");
                    progressLoginInfo.progressConfig = new ProgressConfig
                    {
                        Name = clientIdSplit[0],
                        Operations = ["IIOTS.EdgeDriver"],
                        EquConfigs = [equConfig]
                    };
                }
                var equConfigF = progressLoginInfo?.progressConfig?.EquConfigs.FirstOrDefault(p => p.EQU == equConfig.EQU);
                if (equConfigF != null)
                {
                    progressLoginInfo?.progressConfig?.EquConfigs.Remove(equConfigF);
                }
                progressLoginInfo?.progressConfig?.EquConfigs.Add(equConfig);
                return true;
            }
        }
        /// <summary>
        /// 删除设备配置
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="equConfig"></param>
        /// <returns></returns>
        public bool RemoveEquConfig(string clientId, EquConfig equConfig)
        {
            using (_EdgeLoginInfo.Write())
            {
                ProgressLoginInfo? progressLoginInfo = _EdgeLoginInfo.Data?.
             ProgressLoginInfos.
             FirstOrDefault(p => p.ClientId == clientId);
                if (progressLoginInfo != null
                    && progressLoginInfo.progressConfig != null)
                {
                    var removeEquConfigs = progressLoginInfo.
                        progressConfig.
                        EquConfigs.
                        Where(p => p.EQU == equConfig.EQU)
                        .ToArray();

                    for (int i = 0; i < removeEquConfigs.Length; i++)
                    {
                        progressLoginInfo.progressConfig.EquConfigs.Remove(removeEquConfigs[i]);
                    }
                }
                return true;
            }
        }
        /// <summary>
        /// 添加登录信息
        /// </summary>
        /// <param name="driverLoginInfo"></param>
        /// <returns></returns>
        public bool AddDriverLoginInfo(ProgressLoginInfo driverLoginInfo)
        {
            using (_EdgeLoginInfo.Write())
            {
                if (!(_EdgeLoginInfo.Data?.ProgressLoginInfos.Any(p => p.ClientId == driverLoginInfo.ClientId) ?? true))
                {
                    _EdgeLoginInfo.Data?.ProgressLoginInfos.Add(driverLoginInfo);
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// 更新登录信息
        /// </summary>
        /// <param name="driverLoginInfo"></param>
        /// <returns></returns>
        public void UpdateDriverLoginInfo(ProgressLoginInfo driverLoginInfo)
        {
            using (_EdgeLoginInfo.Write())
            {
                var ProgressLoginInfo = _EdgeLoginInfo.Data?.ProgressLoginInfos.FirstOrDefault(p => p.ClientId == driverLoginInfo.ClientId);
                if (ProgressLoginInfo != null)
                {
                    ProgressLoginInfo = driverLoginInfo;
                }
            }
        }
        /// <summary>
        /// 删除进程
        /// </summary>
        /// <param name="ProgressId"></param>
        /// <returns></returns>
        public bool RemoveDriverLoginInfo(string clientId)
        {
            using (_EdgeLoginInfo.Write())
            {
                var ProgressLoginInfo = _EdgeLoginInfo.Data?.ProgressLoginInfos.FirstOrDefault(p => p.ClientId == clientId);
                if (ProgressLoginInfo != null)
                {
                    _EdgeLoginInfo.Data?.ProgressLoginInfos.Remove(ProgressLoginInfo);
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// 获取未运行进程
        /// </summary>
        /// <param name="ProgressIds"></param>
        /// <returns></returns>
        public List<ProgressLoginInfo> ProgressNotRunList()
        {
            using (_EdgeLoginInfo.Read())
            {
                return _EdgeLoginInfo.Data?.ProgressLoginInfos
                    .Where(p => DateTime.Now - p.HeartbeatTime > timeSpan
                     )
                    .ToList() ?? [];
            }
        }
        /// <summary>
        /// 获取运行进程
        /// </summary>
        /// <param name="ProgressIds"></param>
        /// <returns></returns>
        public List<ProgressLoginInfo> ProgressRunList(string? clientId = null)
        {
            using (_EdgeLoginInfo.Read())
            {
                return _EdgeLoginInfo.Data?.ProgressLoginInfos
                    .Where(p => DateTime.Now - p.HeartbeatTime < timeSpan
                    && (clientId == null || clientId == p.ClientId))
                    .ToList() ?? [];
            }
        }
        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            using (_EdgeLoginInfo.Write())
            {
                _EdgeLoginInfo.Data?.ProgressLoginInfos.Clear();
            }
        }
        /// <summary>
        /// 获取登录信息
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public ProgressLoginInfo? GetDriverLoginInfo(string Name)
        {
            using (_EdgeLoginInfo.Read())
            {
                return _EdgeLoginInfo.Data?.ProgressLoginInfos.FirstOrDefault(p => p.Name == Name);
            }
        }
    }
}
