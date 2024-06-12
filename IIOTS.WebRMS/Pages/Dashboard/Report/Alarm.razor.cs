using Microsoft.AspNetCore.Components;
using IIOTS.WebRMS.Models;
using IIOTS.Util.Infuxdb2;
using IIOTS.Util;
using AntDesign.TableModels; 

namespace IIOTS.WebRMS.Pages.Dashboard.Report
{
    public partial class Alarm : ComponentBase, IDisposable
    {

        [Inject]
        private IInfuxdb Infuxdb { get; set; } = default!;
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
        /// 报警数据
        /// </summary>
        private EquAlarm[] EquAlarms = []; 
        long _total = 0;
        bool tableLoad = false;
        /// <summary>
        /// 页面初始化
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        private async Task OnTableChange(QueryModel<EquAlarm> query)
        {
            tableLoad = true;
            var countFlux = Flux
             .From("Alarms")
             .Import("strings")
             .Import("regexp")
             .Range("-90d")
             .Pivot();
            //查询数据
            var tablesFlux = Flux
            .From("Alarms")
            .Import("strings")
            .Import("regexp")
            .Range("-90d")
            .Pivot();
            foreach (var filterModel in query.FilterModel)
            {
                FnBody fnBody = FnBody.R;
                string filterName = filterModel.FieldName;
                bool isfirst = true;
                foreach (var filter in filterModel.Filters)
                {
                    string filterCompareOperator = filter.FilterCompareOperator.ToString(); 
                    FnBody filterfnBody = filterCompareOperator switch
                    {
                        "Contains" => FnBody.R.ColumnContains(filterName, filter.Value?.ToString() ?? ""),
                        "Equals" => FnBody.R.ColumnEquals(filterName, filter.Value),
                        "NotEquals" => FnBody.R.ColumnNotEquals(filterName, filter.Value),
                        "StartsWith" => FnBody.R.ColumnStartsWith(filterName, filter.Value?.ToString() ?? ""),
                        "EndsWith" => FnBody.R.ColumnEndsWith(filterName, filter.Value?.ToString() ?? ""),
                        "GreaterThan" => FnBody.R.ColumnGreaterThan(filterName, filter.Value?.ToString() ?? ""),
                        "LessThan" => FnBody.R.ColumnLessThan(filterName, filter.Value?.ToString() ?? ""),
                        "GreaterThanOrEquals" => FnBody.R.ColumnGreaterThanOrEquals(filterName, filter.Value?.ToString() ?? ""),
                        "LessThanOrEquals" => FnBody.R.ColumnLessThanOrEquals(filterName, filter.Value?.ToString() ?? ""),
                        _ => throw new NotImplementedException()
                    };
                    if (isfirst)
                    {
                        fnBody.Then($"({filterfnBody})");
                        isfirst = false;
                    }
                    else if (filter.FilterCondition.ToString() == "And")
                    {
                        fnBody.And(filterfnBody);
                    }
                    else
                    {
                        fnBody.Or(filterfnBody);
                    }
                }
                tablesFlux.Filter(fnBody);
                countFlux.Filter(fnBody);
            }
            var count = await Infuxdb.QueryAsync(countFlux.Count("equ"));
            _total = count.FirstOrDefault()?[0].Values.Last()?.ToLong() ?? 0;
            var sortName = query.SortModel.FirstOrDefault(p => p.Sort == "descend")?.FieldName;
            if (sortName != null)
            {
                tablesFlux.Sort(Columns.Create(sortName), desc: true);
            }
            else
            {
                sortName = query.SortModel.FirstOrDefault(p => p.Sort == "ascend")?.FieldName;
                if (sortName != null)
                {
                    tablesFlux.Sort(Columns.Create(sortName), desc: false);
                }
            }
            tablesFlux.Limit(query.PageSize, (query.PageIndex - 1) * query.PageSize);

            var tables = await Infuxdb.QueryAsync(tablesFlux);
            //转换到实体
            EquAlarms = tables.Count > 0 ? tables.Single().ToModels<EquAlarm>() : [];
            //结束设置表单加载状态 
            tableLoad = false;

        }
    }
}
