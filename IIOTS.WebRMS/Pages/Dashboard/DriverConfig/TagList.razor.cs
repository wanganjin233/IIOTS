using IIOTS.Util;
using IIOTS.WebRMS.Models;
using Microsoft.AspNetCore.Components;
using Blazored.LocalStorage;
using Microsoft.JSInterop;
using AntDesign;

namespace IIOTS.WebRMS.Pages.Dashboard.DriverConfig
{
    public partial class TagList : ComponentBase
    {
        [Inject]
        private NotificationService _notice { get; set; } = default!;
        [Inject]
        private ILocalStorageService localStorage { get; set; } = default!;
        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject]
        private IFreeSql FreeSql { get; set; } = default!;
        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;
        [Parameter]
        public EventCallback<TagGroupEntity> SelectTagGroup { get; set; }
        [Parameter]
        public bool IsDisabled { get; set; } = false;
        /// <summary>
        /// Tag组Id
        /// </summary>
        [Parameter]
        public string? TagGid { get; set; }

        List<TagGroupEntity> tagGroupEntities = [];
        /// <summary>
        ///初始化
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            await GetPage();
            await base.OnInitializedAsync();
        }
        bool tableLoad = true;
        /// <summary>
        /// 页面刷新
        /// </summary>
        private async Task GetPage()
        {
            tableLoad = true;
            tagGroupEntities = await FreeSql
            .Select<TagGroupEntity>()
            .WhereIf(TagGid != null, p => p.Id.ToString() == TagGid)
            .ToListAsync();
            tableLoad = false;
        }
        #region 增加修改操作
        private TagGroupEntity tagGroupEntitie = new();
        bool _editBoxVisible = false;
        bool _editBoxLoading = false;
        private async Task AddOrUpdateTagGroup()
        {
            if (await FreeSql
            .InsertOrUpdate<TagGroupEntity>()
            .SetSource(tagGroupEntitie)
            .ExecuteAffrowsAsync() > 0)
            {
                await GetPage();
            }

        }
        #endregion
        #region 删除弹窗
        /// <summary>
        /// 删除点位组
        /// </summary>
        /// <param name="id"></param>
        private async Task Detect(TagGroupEntity tagGroup)
        {
            if (FreeSql
                .GetRepository<TagGroupEntity>()
                .DeleteCascadeByDatabase(p => p.Id == tagGroup.Id)
                .Count > 0)
            {
                await GetPage();
            }
        }
        #endregion
        #region 编辑
        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="id"></param>
        private void Edit(TagGroupEntity tagGroup)
        {
            tagGroupEntitie = tagGroup.DeepClone() ?? new();
            _editBoxVisible = true;
        }
        #region 删除弹窗
        /// <summary>
        ///流程编辑
        /// </summary>
        /// <param name="id"></param>
        private async Task EditFlow(TagGroupEntity tagGroup)
        {
            string? token = await NodeRedApi.GetTokenAsync();
            string? localStorageToken = await localStorage.GetItemAsync<string>("auth-tokens-nodeRed");
            if (token != null && (!localStorageToken?.Contains(token) ?? true))
            {
                await localStorage.SetItemAsync("auth-tokens-nodeRed", new
                {
                    access_token = token,
                    expires_in = 604800,
                    token_type = "Bearer"
                });
            }
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", "editor-language", "zh-CN");
            string? flow = null;
            if (tagGroup.FlowId == null)
            {
                tagGroup.FlowId = await NodeRedApi.CreateFlowAsync(tagGroup.TagGName);
                if (tagGroup.FlowId != null)
                {
                    flow = tagGroup.FlowId;
                    await FreeSql
                     .Update<TagGroupEntity>()
                     .SetSource(tagGroup)
                     .ExecuteAffrowsAsync();
                }
            }
            else
            {
                flow = await NodeRedApi.GetFlowAsync(tagGroup.FlowId);
                if (await NodeRedApi.GetFlowAsync(tagGroup.FlowId) != null)
                {
                    flow = tagGroup.FlowId;
                }
            }
            if (flow != null)
            {
                List<string>? tabFlowIds = await NodeRedApi.GetTabFlowIdsAsync();
                if (tabFlowIds == null)
                {
                    await _notice.Error(new NotificationConfig()
                    {
                        Message = "异常",
                        Description = "打开流程失败"
                    });
                    return;
                }
                else
                {
                    var hiddenTabs = tabFlowIds
                        .Where(p => p != tagGroup.FlowId)
                        .ToDictionary(p => p, _ => true);
                    await localStorage.SetItemAsync("hiddenTabs", hiddenTabs);
                    NavigationManager.NavigateTo($"/Flow/{tagGroup.FlowId}");
                }
            }
        }
        #endregion 
        #endregion
    }
}
