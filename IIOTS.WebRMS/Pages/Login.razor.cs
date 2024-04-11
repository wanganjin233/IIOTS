using IIOTS.Models;
using IIOTS.WebRMS.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;

namespace IIOTS.WebRMS.Pages.Dashboard.NodePanel
{
    public partial class Login : ComponentBase
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

            return base.OnInitializedAsync();
        }
    }

}
