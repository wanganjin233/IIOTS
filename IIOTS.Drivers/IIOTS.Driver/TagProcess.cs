using IIOTS.Enum;
using IIOTS.Models;
using IIOTS.Util;
using Newtonsoft.Json;
using System.Text;

namespace IIOTS.Driver
{
    [JsonObject(MemberSerialization.OptOut)]
    public class TagProcess : Tag
    {
        /// <summary>
        /// 使用的驱动
        /// </summary>
        protected internal BaseDriver? baseDriver { get; set; }
        /// <summary>
        /// 使用的驱动
        /// </summary>
        [JsonIgnore]
        public BaseDriver? BaseDriver { get => baseDriver; }
        /// <summary>
        /// 原始值数据
        /// </summary>
        private object? OriginalData;
        /// <summary>
        /// 值
        /// </summary>
        private object? _Value;
        private object? zoomValue;
        /// <summary>
        /// 缩放后的值
        /// </summary>
        public override object? ZoomValue
        {
            get
            {
                return zoomValue;
            }
            set
            {
                if (value != null)
                {
                    if (DataType == TagTypeEnum.String || DataType == TagTypeEnum.Boole)
                    {
                        Value = value;
                    }
                    else
                    {
                        Value = BitConverter.GetBytes(Convert.ToDouble(value.ToString()) * Magnification);
                    }
                }
            }
        }
        /// <summary>
        /// 变量值
        /// </summary>
        public override object? Value
        {
            get
            {
                return _Value;
            }
            set
            {
                try
                {
                    if (value != null)
                    {
                        if (baseDriver == null)
                        {
                            _Value = value;
                        }
                        else
                        {
                            baseDriver.Write(TagName, value);
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }
        public object? SetValue
        {
            set
            {
                if ((!OriginalData?.Equals(value)) ?? true)
                {
                    OriginalData = value;
                    oldValue = _Value;
                    if (DataType == TagTypeEnum.StringArray)
                    {
                        if (value is string[])
                        {
                            _Value = (value as string[])?.ArrayToStr();
                        }
                        else
                        {
                            _Value = value;
                        }
                    }
                    else
                    {
                        _Value = value;
                    }
                    ChangeTime = DateTime.Now;
                    ThreadPool.QueueUserWorkItem(p => SendValueChangeEvent());
                }
            }
        }


        /// <summary>
        /// 更新点位值
        /// </summary>
        public byte[]? UpdateValue
        {
            set
            {
                try
                {
                    if (value != null)
                    {
                        timestamp = DateTime.Now;
                    }
                    if ((value != null || OriginalData != null)
                        && (!(OriginalData as byte[])?.Equalsbytes(value) ?? true))
                    {
                        OriginalData = value;
                        byte[]? itemValue = value?.DataSequence(Sort);
                        oldValue = _Value;
                        _Value = itemValue == null ? null : DataType switch
                        {
                            TagTypeEnum.Boole => BitConverter.ToBoolean(itemValue),
                            TagTypeEnum.Ushort => BitConverter.ToUInt16(itemValue),
                            TagTypeEnum.Short => BitConverter.ToInt16(itemValue),
                            TagTypeEnum.Uint => BitConverter.ToUInt32(itemValue),
                            TagTypeEnum.Int => BitConverter.ToInt32(itemValue),
                            TagTypeEnum.Float => BitConverter.ToSingle(itemValue),
                            TagTypeEnum.Double => BitConverter.ToDouble(itemValue),
                            TagTypeEnum.Ulong => BitConverter.ToUInt64(itemValue),
                            TagTypeEnum.Long => BitConverter.ToInt64(itemValue),
                            TagTypeEnum.String => Encoding.GetEncoding(Coding.ToString()).GetString(itemValue).Replace("\0", ""),
                            _ => throw new NotImplementedException("无法找到合适的转换")
                        };
                        zoomValue = DataType switch
                        {
                            TagTypeEnum.Ushort => (ushort?)_Value / Magnification,
                            TagTypeEnum.Short => (short?)_Value / Magnification,
                            TagTypeEnum.Uint => (uint?)_Value / Magnification,
                            TagTypeEnum.Int => (int?)_Value / Magnification,
                            TagTypeEnum.Ulong => (ulong?)_Value / Magnification,
                            TagTypeEnum.Long => (long?)_Value / Magnification,
                            TagTypeEnum.Float => (float?)_Value / Magnification,
                            TagTypeEnum.Double => (double?)_Value / Magnification,
                            _ => null
                        };
                        ChangeTime = DateTime.Now;
                        ThreadPool.QueueUserWorkItem(p => SendValueChangeEvent());
                    }
                }
                catch (Exception e)
                {

                }
            }
        }
        private DateTime timestamp = DateTime.MinValue;
        /// <summary>
        /// 时间戳
        /// </summary>
        public override DateTime Timestamp { get => timestamp; set { timestamp = value; } }
        /// <summary>
        /// 质量戳
        /// </summary>
        public override QualityTypeEnum Quality
        {
            get
            {
                if (_Value == null)
                {
                    return QualityTypeEnum.Bad;
                }
                else if (timestamp.AddMilliseconds(5000) > DateTime.Now)
                {
                    return QualityTypeEnum.Good;
                }
                else
                {
                    return QualityTypeEnum.TimeOut;
                }
            }
        }
        /// <summary>
        /// 私有旧值
        /// </summary>
        private object? oldValue;
        /// <summary>
        /// 上次值
        /// </summary>
        public override object? OldValue
        {
            get
            {
                return oldValue;
            }
            set
            {
                if (baseDriver == null)
                {
                    oldValue = value;
                }
            }
        }
    }
}
