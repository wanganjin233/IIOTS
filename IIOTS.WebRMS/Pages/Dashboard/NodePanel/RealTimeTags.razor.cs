using IIOTS.Util;
using IIOTS.WebRMS.Services;
using Microsoft.AspNetCore.Components;
using MQTTnet.Protocol;
using MQTTnet;
using System.Collections.Concurrent;
using IIOTS.Models;
using IIOTS.WebRMS.Models;
using IIOTS.Enum;
using System.Text;

namespace IIOTS.WebRMS.Pages.Dashboard.NodePanel
{
    public partial class RealTimeTags : IDisposable
    {
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

        private bool Refresh = true;
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
            MqttClientService.UnSubscribe($"ValueChange/{Equ}/#");
            MqttClientService.Dispose();
            GC.SuppressFinalize(this);
        }
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
    }
}
