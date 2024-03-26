using IIOTS.Util;
using IIOTS.WebRMS.Models;
using Microsoft.AspNetCore.Components;

namespace IIOTS.WebRMS.Pages.Dashboard.DriverConfig
{
    public partial class TagList
    {
        [Inject]
        private IFreeSql FreeSql { get; set; } = default!;
        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;
        [Parameter]
        public EventCallback<TagGroupEntity> SelectTagGroup { get; set; }
        [Parameter]
        public bool IsDisabled { get; set; } = false;
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
            .ToListAsync();
            tableLoad = false;
        }
        #region 增加修改操作
        private TagGroupEntity tagGroupEntitie = new();
        bool _editBoxVisible = false;
        bool _editBoxLoading = false;
        private async Task AddOrUpdateTagGroup()
        {
            await FreeSql
            .InsertOrUpdate<TagGroupEntity>()
            .SetSource(tagGroupEntitie)
            .ExecuteAffrowsAsync();
            await GetPage();
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
        #endregion
    }
}
