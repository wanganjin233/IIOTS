using IIOTS.Models;
using IIOTS.Util;
using IIOTS.WebRMS.Models;
using Microsoft.AspNetCore.Components;
using MQTTnet.Protocol;
using MQTTnet; 
using IIOTS.WebRMS.Services;

namespace IIOTS.WebRMS.Pages.Dashboard.NodePanel
{
    public partial class ProgressInfo : IDisposable
    {
        [Parameter]
        public required string EdgeId { get; set; }

        [Inject]
        private IMqttClientService MqttClientService { get; set; } = default!;

        [Inject]
        private IFreeSql FreeSql { get; set; } = default!; 
        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        /// <summary>
        ///初始化
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            await GetPage();
            MqttClientService.ApplicationMessageReceived += ApplicationMessageReceived;
            MqttClientService.Subscribe(new MqttTopicFilterBuilder()
           .WithTopic($"EdgeLoginInfo/{EdgeId}")
           .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
           .Build());
            await InvokeAsync(StateHasChanged);
            await base.OnInitializedAsync();
        }
        private EdgeLoginInfo edgeLoginInfo { get; set; } = new();
        private async void ApplicationMessageReceived(string topic, string msg)
        {
            edgeLoginInfo = msg.ToObject<EdgeLoginInfo>();
            await InvokeAsync(StateHasChanged);
        }
        public void Dispose()
        {
            MqttClientService.ApplicationMessageReceived -= ApplicationMessageReceived;
            MqttClientService.UnSubscribe($"EdgeLoginInfo/{EdgeId}");
            MqttClientService.Dispose();
            GC.SuppressFinalize(this);
        }
        private List<ProgressConfigEntity> progressConfigEntities = new List<ProgressConfigEntity>();
        #region 页面展示
        int _pageIndex = 1;
        int _pageSize = 10;
        long _total = 0;
        bool tableLoad = true;
        /// <summary>
        /// 页面刷新
        /// </summary>
        private async Task GetPage()
        {
            tableLoad = true;
            progressConfigEntities = await FreeSql
            .Select<ProgressConfigEntity>()
            .Where(p => p.Gname == EdgeId)
            .Count(out _total)
            .Page(_pageIndex, _pageSize)
            .ToListAsync();
            tableLoad = false;
        }
        /// <summary>
        /// 获取启动时间
        /// </summary>
        /// <param name="progressConfig"></param>
        /// <returns></returns>
        private string GetStartTime(ProgressConfigEntity progressConfig)
        {
            return (edgeLoginInfo
            .ProgressLoginInfos
            .Find(p => p.ClientId == $"{progressConfig.Id}_{progressConfig.ClientType}")?.StartTime ?? DateTime.MinValue)
            .ToString();
        }
        /// <summary>
        /// 更新进程状态
        /// </summary>
        /// <param name="progressConfig"></param>
        /// <returns></returns>
        private void UpdateIsUse(ProgressConfigEntity progressConfig, bool isUse)
        {
            progressConfig.IsUse = isUse;
            if (isUse)
            {
                ReStartProgress(progressConfig);
            }
            else
            {
                RemoveProgress(progressConfig);
            }
            FreeSql
            .Update<ProgressConfigEntity>()
            .Where(p => p.Id == progressConfig.Id)
            .Set(p => p.IsUse, isUse)
            .ExecuteAffrows();

        }
        #endregion
        #region 删除弹窗
        /// <summary>
        /// 删除进程
        /// </summary>
        /// <param name="progressConfig"></param>
        /// <returns></returns>
        private async Task Detect(ProgressConfigEntity progressConfig)
        {
            var DeleteProgressConfigAffrows = await FreeSql
             .Delete<ProgressConfigEntity>(progressConfig)
             .ExecuteAffrowsAsync();
            if (DeleteProgressConfigAffrows > 0)
            {
                RemoveProgress(progressConfig);
                await GetPage();
            }
        }
        #endregion
        #region 增加修改操作
        ProgressConfigEntity AddProgressConfig = new();
        bool _editBoxVisible = false;
        bool _editBoxLoading = false;
        private async Task AddOrUpdateProgress()
        {
            AddProgressConfig.Gname = EdgeId;
            await FreeSql
            .InsertOrUpdate<ProgressConfigEntity>()
            .SetSource(AddProgressConfig)
            .ExecuteAffrowsAsync();
            await GetPage();
        }
        #endregion
        /// <summary>
        /// 重起进程
        /// </summary>
        /// <param name="progressConfig"></param>
        private void ReStartProgress(ProgressConfigEntity progressConfig)
        {
            MqttClientService.Publish($"EdgeCore/{EdgeId}/Progress/AddProgress;{progressConfig.Id}"
            , new ProgressLoginInfo()
            {
                Name = progressConfig.Id.ToString(),
                ClientType = progressConfig.ClientType
            }.ToJson()
            , false);
        }
        /// <summary>
        /// 删除进程
        /// </summary>
        /// <param name="progressConfig"></param>
        private void RemoveProgress(ProgressConfigEntity progressConfig)
        {
            MqttClientService
            .Publish($"EdgeCore/{EdgeId}/Progress/RemoveProgress"
            , $"{progressConfig.Id}_{progressConfig.ClientType}"
            , false);
        }

    }
}
