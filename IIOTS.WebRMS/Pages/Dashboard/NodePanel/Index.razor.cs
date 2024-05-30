using IIOTS.Models;
using IIOTS.Util;
using IIOTS.WebRMS.Services;
using Microsoft.AspNetCore.Components;
using MQTTnet.Protocol;
using MQTTnet;
using System.Collections.Concurrent;

namespace IIOTS.WebRMS.Pages.Dashboard.NodePanel
{
    public partial class Index : ComponentBase, IDisposable
    {
        [Inject]
        private IMqttClientService MqttClientService { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;
        /// <summary>
        /// 边缘节点信息
        /// </summary>
        private ConcurrentDictionary<string, EdgeLoginInfo> edgeLoginInfos = new();
        /// <summary>
        ///初始化页面
        /// </summary>
        /// <returns></returns>
        protected override Task OnInitializedAsync()
        {
            MqttClientService.Subscribe(new MqttTopicFilterBuilder()
               .WithTopic($"EdgeLoginInfo/#")
               .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
               .Build());
            MqttClientService.ApplicationMessageReceived += ApplicationMessageReceived;

            InvokeAsync(StateHasChanged);
            return base.OnInitializedAsync();
        }
        /// <summary>
        /// MQTT接收事件
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        private async void ApplicationMessageReceived(string topic, string msg)
        {
            EdgeLoginInfo? edgeLoginInfo = msg.ToObject<EdgeLoginInfo>();
            if (edgeLoginInfo != null && edgeLoginInfo.EdgeID != null)
            {
                edgeLoginInfos[edgeLoginInfo.EdgeID] = edgeLoginInfo;
                await InvokeAsync(StateHasChanged);
            }
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            MqttClientService.ApplicationMessageReceived -= ApplicationMessageReceived;
            MqttClientService.UnSubscribe($"EdgeLoginInfo/#");
            MqttClientService.Dispose();
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 获取运行的节点
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, EdgeLoginInfo> GetRunEdges()
        {
            return edgeLoginInfos.Where(p => p.Value.State).ToDictionary();
        }
    }

}
