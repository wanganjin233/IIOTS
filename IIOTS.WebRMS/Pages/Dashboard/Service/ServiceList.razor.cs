using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;
using IIOTS.Util;
using IIOTS.WebRMS.Models;
using IIOTS.WebRMS.Services;

namespace IIOTS.WebRMS.Pages.Dashboard.Service
{
    public partial class ServiceList : ComponentBase
    {
        [Inject]
        private IMqttClientService MqttClientService { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private IFreeSql FreeSql { get; set; } = default!;
        /// <summary>
        /// 进程类型
        /// </summary>
        [Parameter]
        public required string ClientType { get; set; }
        /// <summary>
        /// 节点ID
        /// </summary>
        [Parameter]
        public required string EdgeId { get; set; }
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
        /// 节点配置数据
        /// </summary>
        private List<NodeServiceEntity> NodeServiceEntitys = [];
        /// <summary>
        /// 选中的行
        /// </summary>
        private IEnumerable<NodeServiceEntity> selectedRows = [];
        int _pageIndex = 1;
        int _pageSize = 10;
        long _total = 0;
        bool tableLoad = true;
        NodeServiceEntity NodeService = new();
        bool _editBoxVisible = false;
        bool _editBoxLoading = false;
        bool _editNode = false;
        /// <summary>
        /// 页面初始化
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            await GetPage();
            await base.OnInitializedAsync();
        }
        /// <summary>
        /// 页面刷新
        /// </summary>
        private async Task GetPage()
        {
            tableLoad = true;
            NodeServiceEntitys = await FreeSql
            .Select<NodeServiceEntity>()
            .Count(out _total)
            .Page(_pageIndex, _pageSize)
            .ToListAsync();
            tableLoad = false;
        }

        #region 操作事件  
        private async Task AddOrUpdateNodeService()
        {
            _editNode=true;
            await FreeSql
            .InsertOrUpdate<NodeServiceEntity>()
            .SetSource(NodeService)
            .ExecuteAffrowsAsync();
            _editBoxVisible = false;
            await GetPage();
        }
        /// <summary>
        /// 编辑设备配置
        /// </summary>
        /// <param name="nodeServiceEntity"></param>
        private void EditNodeService(NodeServiceEntity nodeServiceEntity)
        {
            _editNode = false;
            NodeService = nodeServiceEntity.DeepClone() ?? new NodeServiceEntity();
            _editBoxVisible = true;
        }
        /// <summary>
        /// 删除设备配置
        /// </summary>
        /// <param name="equConfigEntity"></param>
        /// <returns></returns>
        private async Task DetectNodeService(NodeServiceEntity nodeServiceEntity)
        {
            tableLoad = true;
            await FreeSql
               .Delete<NodeServiceEntity>(nodeServiceEntity)
               .ExecuteAffrowsAsync();
            await GetPage();
            tableLoad = false;
        }
        #endregion
    }
}
