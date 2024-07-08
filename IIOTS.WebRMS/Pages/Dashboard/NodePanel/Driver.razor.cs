using Microsoft.AspNetCore.Components;
using MQTTnet.Protocol;
using MQTTnet;
using System.Collections.Concurrent;
using IIOTS.Util;
using IIOTS.WebRMS.Models;
using IIOTS.Models;
using IIOTS.WebRMS.Extensions;
using IIOTS.WebRMS.Services; 

namespace IIOTS.WebRMS.Pages.Dashboard.NodePanel
{
    public partial class Driver : ComponentBase, IDisposable
    {
        [Inject]
        private IMqttClientService MqttClientService { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private IFreeSql FreeSql { get; set; } = default!; 
        /// <summary>
        /// 进程ID
        /// </summary>
        [Parameter]
        public required string ProgressId { get; set; }
        /// <summary>
        /// 设备状态
        /// </summary>
        private readonly ConcurrentDictionary<string, bool> DriverState = new();
        /// <summary>
        /// 进程信息
        /// </summary>
        private ProgressConfigEntity progressConfig = new();
        /// <summary>
        /// 节点配置数据
        /// </summary>
        private List<EquConfigEntity> EquConfigEntitys = [];
        /// <summary>
        /// 选中的行
        /// </summary>
        private IEnumerable<EquConfigEntity> selectedRows = [];
        int _pageIndex = 1;
        int _pageSize = 10;
        long _total = 0;
        bool tableLoad = true;
        EquConfigEntity EquConfig = new();
        bool _editBoxVisible = false;
        bool _editBoxLoading = false;
        /// <summary>
        /// 页面初始化
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            await GetPage();
            //查询进程信息
            progressConfig = FreeSql
                     .Select<ProgressConfigEntity>()
                     .Where(p => p.Id.ToString() == ProgressId)
                     .First();
            MqttClientService.Subscribe(new MqttTopicFilterBuilder()
                                            .WithTopic($"DriverStateChange/#")
                                            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                                            .Build());
            MqttClientService.ApplicationMessageReceived += ApplicationMessageReceived;
            await InvokeAsync(StateHasChanged);
            await base.OnInitializedAsync();
        }
        /// <summary>
        /// MQTT接收事件
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        private async void ApplicationMessageReceived(string topic, string msg)
        {
            if (DriverState.ContainsKey(topic.Split("/").Last()))
            { 
                DriverState[topic.Split("/").Last()] = msg.ToObject<bool>();
                await InvokeAsync(StateHasChanged);
            } 
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            MqttClientService.ApplicationMessageReceived -= ApplicationMessageReceived;
            MqttClientService.UnSubscribe($"DriverStateChange/#");
            MqttClientService.Dispose();
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 页面刷新
        /// </summary>
        private async Task GetPage()
        {
            tableLoad = true;
            EquConfigEntitys = await FreeSql
            .Select<EquConfigEntity>()
            .LeftJoin(p => p.TagGroupEntity.Id == p.TagGroupId)
            .LeftJoin(p => p.ProgressConfigEntity.Id == p.ProgressId)
            .WhereIf(ProgressId != null, p => p.ProgressId.ToString() == ProgressId)
            .ToListAsync();
            foreach (var equConfig in EquConfigEntitys)
            {
                DriverState[equConfig.EQU] = false;
            }
            tableLoad = false;
        }
        #region 操作节点
        /// <summary>
        /// 设置Tag组
        /// </summary>
        /// <param name="tagGroup"></param>
        private void SetTagGroup(TagGroupEntity tagGroup)
        {
            EquConfig.TagGroupId = tagGroup.Id;
            EquConfig.TagGroupEntity = tagGroup;
        }
        private async Task AddEquConfig()
        {
            _editBoxLoading = true;
            EquConfig.ProgressId = long.Parse(ProgressId);
            await FreeSql
            .InsertOrUpdate<EquConfigEntity>()
            .SetSource(EquConfig)
            .ExecuteAffrowsAsync();
            await GetPage();
            _editBoxLoading = false;
            _editBoxVisible = false;
        }
        /// <summary>
        ///  设备启用状态修改
        /// </summary>
        /// <param name="equConfig"></param>
        /// <param name="tags"></param>
        private void SwitchEqu(EquConfigEntity equConfig, List<TagConfig> tags)
        {
            string topic = $"EdgeCore/{progressConfig.Gname}/Equ/DeployEqu;{equConfig.EQU}";
            string id = $"{progressConfig.Id}_{progressConfig.ClientType}";
            if (equConfig.IsUse)
            {
                MqttClientService.Publish(topic, new Operate<EquConfig>()
                {
                    Id = id,
                    Content = new EquConfig()
                    {
                        Enable = true,
                        ConnectionString = equConfig.ConnectionString,
                        Description = equConfig.Description ?? string.Empty,
                        EQU = equConfig.EQU,
                        DriverType = equConfig.TagGroupEntity.DriverType,
                        ScanRate = equConfig.ScanRate,
                        Tags = tags
                    }
                }.ToJson(), true);
            }
            else
            {
                MqttClientService.Publish(topic, new Operate<EquConfig>()
                {
                    Id = id,
                    Content = new EquConfig()
                    {
                        Enable = false,
                        EQU = equConfig.EQU
                    }
                }.ToJson(), true);
            }
        }
        #endregion

        #region 操作事件 
        private void Apply()
        {
            //获取设备配置包含的点位并分组
            var tagGroup = FreeSql
                        .Select<TagConfigEntity>()
                        .Where(p => selectedRows.Any(a => a.TagGroupId == p.GID))
                        .ToList()
                        .GroupBy(p => p.GID);
            //遍历设备配置
            foreach (var equConfig in selectedRows)
            {
                //获取对应点位
                var tags = tagGroup
                .FirstOrDefault(p => p.Key == equConfig.TagGroupId)
                ?.ToList()
                .ToTag();
                if (tags != null)
                {
                    SwitchEqu(equConfig, tags);
                }
            }
            selectedRows = [];
        }
        /// <summary>
        /// 设备配置启用状态修改
        /// </summary>
        /// <param name="equConfigEntity"></param>
        private void SwitchEquConfig(EquConfigEntity equConfig, bool isUse)
        {
            tableLoad = true;
            equConfig.IsUse = isUse;
            FreeSql
            .Update<EquConfigEntity>()
            .Where(p => p.Id == equConfig.Id)
            .Set(p => p.IsUse, isUse)
            .ExecuteAffrows();
            tableLoad = false;
        }

        /// <summary>
        /// 编辑设备配置
        /// </summary>
        /// <param name="equConfigEntity"></param>
        private void EditEquConfig(EquConfigEntity equConfigEntity)
        {
            EquConfig = equConfigEntity.DeepClone() ?? new EquConfigEntity();
            _editBoxVisible = true;
        }
        /// <summary>
        /// 删除设备配置
        /// </summary>
        /// <param name="equConfigEntity"></param>
        /// <returns></returns>
        private async Task DetectEquConfig(EquConfigEntity equConfigEntity)
        {
            tableLoad = true;
            await FreeSql
               .Delete<EquConfigEntity>(equConfigEntity)
               .ExecuteAffrowsAsync();
            await GetPage();
            tableLoad = false;
        }
        #endregion
    }
}
