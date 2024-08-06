using Microsoft.AspNetCore.Components;
using IIOTS.WebRMS.Models;
using IIOTS.Util.Infuxdb2;
using AntDesign;
using SharpCifs.Util.Sharpen;
using AntDesign.TableModels;

namespace IIOTS.WebRMS.Pages.Dashboard.Report
{
    public partial class Efficency : ComponentBase, IDisposable
    {

        [Inject]
        private IFreeSql FreeSql { get; set; } = default!;
        [Inject]
        private IMessageService Message { get; set; } = default!;
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
        private DateTime? startDateTime;
        string Equ = string.Empty;
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


        private List<EfficencySum> EfficencySums = [];
        private class EfficencySum
        {
            /// <summary>
            /// 工厂
            /// </summary>
            public string? FACTORYNAME { get; set; }
            /// <summary>
            /// 步骤
            /// </summary>
            public string? STEPNAME { get; set; }
            /// <summary>
            /// 总体稼动率
            /// </summary>
            public double? Efficency
            {
                get
                {
                    if (EQUs.Count == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return EQUs.Sum(p => p.EfficencyData?.Efficency ?? 0) / EQUs.Count;

                    }
                }
            }
            /// <summary>
            /// 设备清单
            /// </summary>
            public List<EQU> EQUs { get; set; } = [];

        } 
        /// <summary>
        /// 设备信息
        /// </summary>
        private class EQU
        {
            /// <summary>
            /// 设备号
            /// </summary>
            public string? DEVICECODE { get; set; }
            /// <summary>
            /// 设备名称
            /// </summary>
            public string? DEVICENAME { get; set; }
            /// <summary>
            /// 稼动率数据
            /// </summary>
            public EfficencyData? EfficencyData { get; set; }

        }
        private async Task OnRowExpand(RowData<EquInfoEntity> rowData)
        {
            if (startDateTime != null)
            {
                try
                {
                    DateTime? startDate = new DateTime(startDateTime.Value.Year, startDateTime.Value.Month, startDateTime.Value.Day, 8, 0, 0);
                    DateTime? stopDate = startDate.Value.AddDays(1);
                    if (startDate != null && stopDate != null)
                    {
                        var tablesFlux = Flux
                            .From("iiots_Status")
                            .Import("strings")
                            .Import("regexp")
                            .Range(DateTime.SpecifyKind((DateTime)startDate, DateTimeKind.Local)
                            , DateTime.SpecifyKind((DateTime)stopDate, DateTimeKind.Local))
                            .Filter(FnBody.R.ColumnExists("equ"))
                            .Filter(FnBody.R.ColumnContains("equ", Equ))
                            .Pivot()
                            .Group(Columns.Create("_measurement"));
                        var tables = await Infuxdb.QueryAsync(tablesFlux);
                        //转换到实体
                        EquStatus[] EquStatus = tables.Count > 0 ? tables.Single().ToModels<EquStatus>() : [];


                        tablesFlux = Flux
                        .From("iiots_Status")
                        .Range(DateTime.SpecifyKind(DateTime.Now.AddDays(-180), DateTimeKind.Local)
                        , DateTime.SpecifyKind(((DateTime)startDate).AddSeconds(-1), DateTimeKind.Local))
                        .Filter(FnBody.R.ColumnExists("equ"))
                        .Filter(FnBody.R.ColumnContains("equ", Equ))
                        .Sort(Columns.Create("_time"), desc: true)
                        .Pivot()
                        .Limit(1);
                        tables = await Infuxdb.QueryAsync(tablesFlux);
                        var equStatu = tables.Count > 0 ? tables.Single().ToModels<EquStatus>() : [];
                        List<EfficencyData> efficencyDatas = [];
                        foreach (var equStatus in EquStatus.GroupBy(p => p.equ))
                        {
                            var equStatusArray = equStatus.ToArray();
                            EfficencyData efficencyData = new()
                            {
                                Equ = equStatus.Key
                            };
                            for (int i = 0; equStatus.Count() >= i; i++)
                            {
                                double second = 0;
                                var status = 0;
                                if (i == 0)
                                {
                                    var firstStatus = equStatu.FirstOrDefault(p => p.equ == efficencyData.Equ);
                                    if (firstStatus == null)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        second = ((DateTime)equStatusArray[i]._time - ((DateTime)startDate)).TotalSeconds;
                                        status = firstStatus.status ?? 0;
                                    }
                                }
                                else if (equStatus.Count() == i)
                                {
                                    second = ((DateTime)stopDate - (DateTime)equStatusArray[i - 1]._time).TotalSeconds;
                                    status = equStatusArray[i - 1].status ?? 0;
                                }
                                else
                                {
                                    second = ((DateTime)equStatusArray[i]._time - (DateTime)equStatusArray[i - 1]._time).TotalSeconds;
                                    status = equStatusArray[i - 1].status ?? 0;
                                }
                                switch (status)
                                {
                                    case 1:
                                        efficencyData.RunTime += second;
                                        break;
                                    case 2:
                                        efficencyData.StandbyTime += second;
                                        break;
                                    case 3:
                                        efficencyData.AlarmTime += second;
                                        break;
                                    case 4:
                                        efficencyData.OffLineTime += second;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            if (efficencyData.RunTime + efficencyData.StandbyTime + efficencyData.AlarmTime + efficencyData.OffLineTime == 0)
                            {
                                efficencyData.Efficency = 0;
                            }
                            else
                            {
                                efficencyData.Efficency = efficencyData.RunTime / (efficencyData.RunTime + efficencyData.StandbyTime + efficencyData.AlarmTime + efficencyData.OffLineTime);
                            }
                            efficencyDatas.Add(efficencyData);
                        }

                        rowData.Data.EfficencyDatas = efficencyDatas.ToArray();
                    }
                }
                catch (Exception e)
                {
                    rowData.Data.EfficencyDatas = [];
                    _ = Message.Error($"查询失败{e.Message}");
                }
            }
            else
            {
                _ = Message.Warning($"请先选择日期！");
            }
        }

        private async Task ReloadData()
        {
            List<EfficencyData> efficencyDatas = [];
            try
            {
                DateTime? startDate = new DateTime(startDateTime.Value.Year, startDateTime.Value.Month, startDateTime.Value.Day, 8, 0, 0);
                DateTime? stopDate = startDate.Value.AddDays(1);
                if (startDate != null && stopDate != null)
                {
                    var tablesFlux = Flux
                        .From("iiots_Status")
                        .Import("strings")
                        .Import("regexp")
                        .Range(DateTime.SpecifyKind((DateTime)startDate, DateTimeKind.Local)
                        , DateTime.SpecifyKind((DateTime)stopDate, DateTimeKind.Local))
                        .Filter(FnBody.R.ColumnExists("equ"))
                        .Filter(FnBody.R.ColumnContains("equ", Equ))
                        .Pivot()
                        .Group(Columns.Create("_measurement"));
                    var tables = await Infuxdb.QueryAsync(tablesFlux);
                    //转换到实体
                    EquStatus[] EquStatus = tables.Count > 0 ? tables.Single().ToModels<EquStatus>() : [];


                    tablesFlux = Flux
                    .From("iiots_Status")
                    .Range(DateTime.SpecifyKind(DateTime.Now.AddDays(-180), DateTimeKind.Local)
                    , DateTime.SpecifyKind(((DateTime)startDate).AddSeconds(-1), DateTimeKind.Local))
                    .Filter(FnBody.R.ColumnExists("equ"))
                    .Sort(Columns.Create("_time"), desc: true)
                    .Pivot()
                    .Limit(1);
                    tables = await Infuxdb.QueryAsync(tablesFlux);
                    var equStatu = tables.Count > 0 ? tables.Single().ToModels<EquStatus>() : [];

                    foreach (var equStatus in EquStatus.GroupBy(p => p.equ))
                    {
                        var equStatusArray = equStatus.ToArray();
                        EfficencyData efficencyData = new()
                        {
                            Equ = equStatus.Key
                        };
                        for (int i = 0; equStatus.Count() >= i; i++)
                        {
                            double second = 0;
                            var status = 0;
                            if (i == 0)
                            {
                                var firstStatus = equStatu.FirstOrDefault(p => p.equ == efficencyData.Equ);
                                if (firstStatus == null)
                                {
                                    break;
                                }
                                else
                                {
                                    second = ((DateTime)equStatusArray[i]._time - ((DateTime)startDate)).TotalSeconds;
                                    status = firstStatus.status ?? 0;
                                }
                            }
                            else if (equStatus.Count() == i)
                            {
                                second = ((DateTime)stopDate - (DateTime)equStatusArray[i - 1]._time).TotalSeconds;
                                status = equStatusArray[i - 1].status ?? 0;
                            }
                            else
                            {
                                second = ((DateTime)equStatusArray[i]._time - (DateTime)equStatusArray[i - 1]._time).TotalSeconds;
                                status = equStatusArray[i - 1].status ?? 0;
                            }
                            switch (status)
                            {
                                case 1:
                                    efficencyData.RunTime += second;
                                    break;
                                case 2:
                                    efficencyData.StandbyTime += second;
                                    break;
                                case 3:
                                    efficencyData.AlarmTime += second;
                                    break;
                                case 4:
                                    efficencyData.OffLineTime += second;
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (efficencyData.RunTime + efficencyData.StandbyTime + efficencyData.AlarmTime + efficencyData.OffLineTime == 0)
                        {
                            efficencyData.Efficency = 0;
                        }
                        else
                        {
                            efficencyData.Efficency = efficencyData.RunTime / (efficencyData.RunTime + efficencyData.StandbyTime + efficencyData.AlarmTime + efficencyData.OffLineTime);
                        }
                        efficencyDatas.Add(efficencyData);
                    }
                }
            }
            catch (Exception e)
            {
                _ = Message.Error($"查询失败{e.Message}");
            }

            var equInfoEntitys = await FreeSql
                .Select<EquInfoEntity>()
                .ToListAsync();
            List<EfficencySum> _EfficencySums = [];
            foreach (var item in equInfoEntitys)
            {
                if (item.STEPNAME != null)
                {
                    EfficencySum? _EfficencySum = _EfficencySums.FirstOrDefault(p => p.STEPNAME == item.STEPNAME);
                    if (_EfficencySum == null)
                    {
                        _EfficencySums.Add(new EfficencySum
                        {
                            STEPNAME = item.STEPNAME,
                            FACTORYNAME = item.FACTORYNAME
                        });
                    }
                    else
                    {
                        EfficencyData? efficencyData = efficencyDatas.FirstOrDefault(p => p.Equ == item.DEVICECODE);
                        if (efficencyData != null)
                        {
                            _EfficencySum.EQUs.Add(new EQU
                            {
                                DEVICECODE = item.DEVICECODE,
                                DEVICENAME = item.DEVICENAME,
                                EfficencyData = efficencyData
                            });
                        }
                    }
                }
            }
            EfficencySums = _EfficencySums;
        }
    }
}
