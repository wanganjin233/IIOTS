using IIOTS.Util;
using IIOTS.WebRMS.Services;
using Microsoft.AspNetCore.Components;
using MQTTnet.Protocol;
using MQTTnet;
using System.Collections.Concurrent;
using IIOTS.Models;
using IIOTS.WebRMS.Models;



namespace IIOTS.WebRMS.Pages.Dashboard.NodePanel
{
    public partial class RealTimeTags : IDisposable
    {/// <summary>
     /// 节点id
     /// </summary>
        [Parameter]
        public string? edgeId { get; set; }
        /// <summary>
        /// 进程id
        /// </summary>
        [Parameter]
        public string? ProgressId { get; set; }
        /// <summary>
        /// 设备编码
        /// </summary>
        [Parameter]
        public string? equ { get; set; }
        IEnumerable<Tag> selectedRows;
        private bool editBoxVisible = false;
        ConcurrentDictionary<string, Tag> tags = new();
        [Inject]
        private IMqttClientService MqttClientService { get; set; } = default!;

        [Inject]
        private IFreeSql FreeSql { get; set; } = default!;
        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        private bool Refresh = true;


        /// <summary>
        ///初始化
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            GetPage();
            MqttClientService.Subscribe(new MqttTopicFilterBuilder()
              .WithTopic($"ValueChange/{equ}/#")
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build());
            MqttClientService.ApplicationMessageReceived += ApplicationMessageReceived;
            _ = Task.Run(async () =>
            {
                while (Refresh)
                {
                    Task.Delay(1000).Wait();
                    await InvokeAsync(StateHasChanged);
                }
            });
            await base.OnInitializedAsync();
        }

        private void ApplicationMessageReceived(string topic, string msg)
        {
            if (tags.ContainsKey(topic) && msg.TryToObject<Tag>(out var tag))
            {
                tags[topic] = tag;
            }
        }
        public void Dispose()
        {
            Refresh = false;
            MqttClientService.ApplicationMessageReceived -= ApplicationMessageReceived;
            MqttClientService.UnSubscribe($"ValueChange/{equ}/#");
            MqttClientService.Dispose();
            GC.SuppressFinalize(this);
        }
        private void GetPage()
        {
            List<TagConfigEntity> tagConfigs = FreeSql.Select<EquConfigEntity>()
              .IncludeMany(p => p.TagGroupEntity.TagConfigEntitys)
              .Where(p => p.EQU == equ)
              .ToList()
              .Select(p => p.TagGroupEntity.TagConfigEntitys)
              .First()
              .ToList();
            foreach (var tagConfig in tagConfigs)
            {
                tags[$"ValueChange/{equ}/{tagConfig.TagName}"] = new Tag
                {
                    TagName = tagConfig.TagName,
                    Address = tagConfig.Address,
                    ClientAccess = tagConfig.ClientAccess,
                    Coding = tagConfig.Coding,
                    EngUnits = tagConfig.EngUnits,
                    Sort = tagConfig.Sort,
                    DataLength = tagConfig.DataLength,
                    DataType = tagConfig.DataType,
                    UpdateMode = tagConfig.UpdateMode,
                    Magnification = tagConfig.Magnification,
                    Description = tagConfig.Description,
                    StationNumber = (byte)tagConfig.StationNumber
                };
            }
        }
        public Operate<string> WriteTagValue = new Operate<string>() { Id = string.Empty, Content = string.Empty };
        public void WriteTag(string tagName) => MqttClientService.Publish($"EdgeCore/all/Equ/WriteTag"
                , new Operate<Tag>()
                {
                    Id = equ,
                    Content = new Tag
                    {
                        TagName = tagName,
                        Value = WriteTagValue.Content
                    }
                }.ToJson()
                , false);
    }
}
