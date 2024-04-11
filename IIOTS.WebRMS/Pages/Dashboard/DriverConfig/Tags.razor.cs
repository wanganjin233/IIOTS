using IIOTS.Models;
using IIOTS.Util;
using IIOTS.WebRMS.Models;
using Microsoft.AspNetCore.Components;

namespace IIOTS.WebRMS.Pages.Dashboard.DriverConfig
{
    public partial class Tags
    {
        /// <summary>
        /// Tag组ID
        /// </summary>
        [Parameter]
        public string? GID { get; set; }

        [Inject]
        private IFreeSql FreeSql { get; set; } = default!;
        /// <summary>
        /// 选中行
        /// </summary>
        private IEnumerable<TagConfigEntity> selectedRows = [];
        /// <summary>
        /// 编辑窗口显示状态
        /// </summary>
        private bool editBoxVisible = false;
        /// <summary>
        /// 所有点位列表
        /// </summary>
        private List<TagConfigEntity> TagConfigs = [];
        /// <summary>
        /// 编辑的点位
        /// </summary>
        private TagConfigEntity tagConfig = new();
        /// <summary>
        ///初始化
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            await GetPage();
            await base.OnInitializedAsync();
        }
        /// <summary>
        /// 获取页面
        /// </summary>
        /// <returns></returns>
        private async Task GetPage()
        {
            TagConfigs = await FreeSql
            .Select<TagConfigEntity>()
            .Where(p => p.GID.ToString() == GID)
            .ToListAsync();
        }
        /// <summary>
        /// 更新或添加点位
        /// </summary>
        /// <returns></returns>
        private async Task UpdataOrAdd()
        {
            if (GID != null)
            {
                tagConfig.GID = GID.ToLong();
                if (await FreeSql
                .InsertOrUpdate<TagConfigEntity>()
                 .SetSource(tagConfig)
                .ExecuteAffrowsAsync() > 0)
                {
                    await GetPage();
                    editBoxVisible = false;
                }
            }
        }
        /// <summary>
        /// 拷贝选中点位
        /// </summary>
        /// <returns></returns>
        private async Task CopyTags()
        {
            List<TagConfigEntity> tagConfigs = [];
            foreach (var selectedTagConfig in selectedRows)
            {
                var tagConfig = selectedTagConfig.DeepClone();
                if (tagConfig != null)
                {
                    tagConfig.TagName += "_Copy";
                    tagConfigs.Add(tagConfig);
                }
            }
            if (await FreeSql
                    .Insert(tagConfigs)
                    .ExecuteAffrowsAsync() > 0)
            {
                selectedRows = [];
                await GetPage();
            };
        }
        /// <summary>
        /// 删除点位
        /// </summary>
        /// <param name="tagConfig"></param>
        /// <returns></returns>
        private async Task DetectTag(TagConfigEntity tagConfig)
        {
            if (await FreeSql
               .Delete<TagConfigEntity>(tagConfig)
               .ExecuteAffrowsAsync() > 0)
            {
                await GetPage();
            };
        }
        /// <summary>
        /// 删除选择点位
        /// </summary>
        /// <returns></returns>
        private async Task DetectTags()
        {
            if (await FreeSql
               .Delete<TagConfigEntity>(selectedRows)
               .ExecuteAffrowsAsync() > 0)
            {
                selectedRows = [];
                await GetPage();
            };
        }

    }
}
