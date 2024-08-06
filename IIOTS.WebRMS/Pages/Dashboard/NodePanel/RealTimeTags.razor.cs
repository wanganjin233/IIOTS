using IIOTS.Util;
using IIOTS.WebRMS.Services;
using Microsoft.AspNetCore.Components;
using MQTTnet.Protocol;
using MQTTnet;
using System.Collections.Concurrent;
using IIOTS.Models;
using IIOTS.WebRMS.Models;
using IIOTS.Enums;
using IIOTS.Util.Infuxdb2;

namespace IIOTS.WebRMS.Pages.Dashboard.NodePanel
{
    public partial class RealTimeTags : ComponentBase, IDisposable
    { 
        /// <summary>
        /// 节点id
        /// </summary>
        [Parameter]
        public string? EdgeId { get; set; }
        /// <summary>
        /// 进程id
        /// </summary>
        [Parameter]
        public string? ProgressId { get; set; }
        /// <summary>
        /// 设备编码
        /// </summary>
        [Parameter]
        public string? Equ { get; set; }

        private bool editBoxVisible = false;
        readonly ConcurrentDictionary<string, Tag> tags = new();
        [Inject]
        private IMqttClientService MqttClientService { get; set; } = default!;

        [Inject]
        private IFreeSql FreeSql { get; set; } = default!;

        [Inject]
        private IInfuxdb Infuxdb { get; set; } = default!;

        private bool Refresh = true; 
        /// <summary>
        /// 点位历史值窗口显示
        /// </summary>
        bool logBoxVisible = false;
        /// <summary>
        /// 点位历史值加载
        /// </summary>
        bool logBoxLoading = false;
        /// <summary>
        /// 点位表格页码
        /// </summary>
        int logPageIndex = 1;
        /// <summary>
        /// 点位明细
        /// </summary>
        EquTag[] equTags = [];
        /// <summary>
        ///初始化
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            GetPage();
            MqttClientService.Subscribe(new MqttTopicFilterBuilder()
              .WithTopic($"ValueChange/{Equ}/#")
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
        /// <summary>
        /// MQTT订阅主题变化事件
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        private void ApplicationMessageReceived(string topic, string msg)
        {
            if (tags.ContainsKey(topic) && msg.TryToObject<Tag>(out var tag))
            {
                tags[topic] = tag;
            }
        } 
        /// <summary>
        /// 获取Tag点位历史记录
        /// </summary>
        /// <param name="tag"></param>
        private async void ShowTagLog( Tag tag)
        {
            //重置页码
            logPageIndex = 1;
            //重置点位历史记录
            equTags = [];
            //显示历史值弹框
            logBoxVisible = true;
            //设置表单为加载状态
            logBoxLoading = true;
            //生成flux语句
            var flux = Flux
            .From("Tags")
            .Range("-90d")
            .Filter(FnBody.R.MeasurementEquals(Equ))
            .Filter(FnBody.R.ColumnEquals("tagName", tag.TagName))
            .Pivot()
            .Sort(Columns.Time, desc: true) 
            .Limit(100);
            //查询数据
            var tables = await Infuxdb.QueryAsync(flux);
            //转换到实体
            equTags = tables.Count > 0 ? tables.Single().ToModels<EquTag>() : [];
            //结束设置表单加载状态
            logBoxLoading = false;
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            Refresh = false;
            MqttClientService.ApplicationMessageReceived -= ApplicationMessageReceived;
            MqttClientService.UnSubscribe($"ValueChange/{Equ}/#");
            MqttClientService.Dispose();
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 获取表格内容
        /// </summary>
        private void GetPage()
        {
            List<TagConfigEntity> tagConfigs = FreeSql.Select<EquConfigEntity>()
              .IncludeMany(p => p.TagGroupEntity.TagConfigEntitys)
              .Where(p => p.EQU == Equ)
              .ToList()
              .Select(p => p.TagGroupEntity.TagConfigEntitys)
              .First()
              .ToList();
            foreach (var tagConfig in tagConfigs)
            {
                tags[$"ValueChange/{Equ}/{tagConfig.TagName}"] = new Tag
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
        public WriteTagContent _WriteTag = new();
        /// <summary>
        /// 写入Tag
        /// </summary>
        private void WriteTag()
        {
            string? _WriteValue = _WriteTag.WriteValue?.ToString();
            if (_WriteValue == null)
            {
                return;
            }
            if (_WriteTag.Type == TagTypeEnum.StringArray)
            {
                _WriteValue = _WriteValue.Replace("\n", ",");
            }
            MqttClientService.Publish($"EdgeCore/all/Equ/WriteTag"
                , new Operate<Tag>()
                {
                    Id = Equ,
                    Content = new Tag()
                    {
                        TagName = _WriteTag.TagName,
                        Value = _WriteValue
                    }
                }.ToJson()
                , false);
        }
        /// <summary>
        /// 打开写入窗口
        /// </summary>
        /// <param name="tag"></param>
        private void OpenWritBox(Tag tag)
        {
            _WriteTag.TagName = tag.TagName;
            _WriteTag.Type = tag.DataType;
            _WriteTag.WriteValue = tag.DataType switch
            {
                TagTypeEnum.Boole => Convert.ToBoolean(tag.Value),
                TagTypeEnum.Ushort => Convert.ToUInt16(tag.Value),
                TagTypeEnum.Short => Convert.ToInt16(tag.Value),
                TagTypeEnum.Uint => Convert.ToUInt32(tag.Value),
                TagTypeEnum.Int => Convert.ToInt32(tag.Value),
                TagTypeEnum.Float => Convert.ToSingle(tag.Value),
                TagTypeEnum.Double => Convert.ToDouble(tag.Value),
                TagTypeEnum.Ulong => Convert.ToUInt64(tag.Value),
                TagTypeEnum.Long => Convert.ToInt64(tag.Value),
                TagTypeEnum.String => tag.Value.ToString(),
                TagTypeEnum.StringArray => tag.Value.ToString().Replace(",", "\n"),
                _ => throw new NotImplementedException("无法找到合适的转换")
            };
            editBoxVisible = true;
        }
        #region 写值转换 
        private bool WriteBoole
        {
            get
            {
                if (_WriteTag.WriteValue is not null and bool)
                {
                    return (bool)_WriteTag.WriteValue;
                }
                else
                {
                    return default;
                }
            }
            set
            {
                _WriteTag.WriteValue = value;
            }
        }
        private short WriteShort
        {
            get
            {
                if (_WriteTag.WriteValue is not null and short)
                {
                    return (short)_WriteTag.WriteValue;
                }
                else
                {
                    return default;
                }
            }
            set
            {
                _WriteTag.WriteValue = value;
            }
        }
        private ushort WriteUshort
        {
            get
            {
                if (_WriteTag.WriteValue is not null and ushort)
                {
                    return (ushort)_WriteTag.WriteValue;
                }
                else
                {
                    return default;
                }
            }
            set
            {
                _WriteTag.WriteValue = value;
            }
        }
        private uint WriteUint
        {
            get
            {
                if (_WriteTag.WriteValue is not null and uint)
                {
                    return (uint)_WriteTag.WriteValue;
                }
                else
                {
                    return default;
                }
            }
            set
            {
                _WriteTag.WriteValue = value;
            }
        }
        private int WriteInt
        {
            get
            {
                if (_WriteTag.WriteValue is not null and int)
                {
                    return (int)_WriteTag.WriteValue;
                }
                else
                {
                    return default;
                }
            }
            set
            {
                _WriteTag.WriteValue = value;
            }
        }
        private float WriteFloat
        {
            get
            {
                if (_WriteTag.WriteValue is not null and float)
                {
                    return (float)_WriteTag.WriteValue;
                }
                else
                {
                    return default;
                }
            }
            set
            {
                _WriteTag.WriteValue = value;
            }
        }
        private double WriteDouble
        {
            get
            {
                if (_WriteTag.WriteValue is not null and double)
                {
                    return (double)_WriteTag.WriteValue;
                }
                else
                {
                    return default;
                }
            }
            set
            {
                _WriteTag.WriteValue = value;
            }
        }
        private ulong WriteUlong
        {
            get
            {
                if (_WriteTag.WriteValue is not null and ulong)
                {
                    return (ulong)_WriteTag.WriteValue;
                }
                else
                {
                    return default;
                }
            }
            set
            {
                _WriteTag.WriteValue = value;
            }
        }
        private long WriteLong
        {
            get
            {
                if (_WriteTag.WriteValue is not null and long)
                {
                    return (long)_WriteTag.WriteValue;
                }
                else
                {
                    return default;
                }
            }
            set
            {
                _WriteTag.WriteValue = value;
            }
        }
        private string WriteString
        {
            get
            {
                if (_WriteTag.WriteValue is not null and string)
                {
                    return (string)_WriteTag.WriteValue;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                _WriteTag.WriteValue = value;
            }
        }
        #endregion
    }
}
