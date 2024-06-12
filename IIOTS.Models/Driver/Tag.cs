using IIOTS.Enums;

namespace IIOTS.Models
{
    public class Tag : TagConfig
    {
        /// <summary>
        /// 西门子DB块
        /// </summary>
        public ushort DbBlock { get; set; }
        /// <summary>
        /// 旧值
        /// </summary>
        public virtual object? OldValue { get; set; }
        /// <summary>
        /// 缩放后的值
        /// </summary>
        public virtual object? ZoomValue { get; set; }
        /// <summary>
        /// 变量值
        /// </summary>
        public virtual object? Value { get; set; }
        /// <summary>
        /// 位置
        /// </summary>
        public uint Location { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public object Type { get; set; } = 0x0;
        /// <summary>
        /// 是否为位地址
        /// </summary>
        public bool IsBit { get; set; } = false;
        /// <summary>
        /// 位地址位置
        /// </summary>
        public int BitLocation { get; set; } = -1;
        /// <summary>
        /// 点位异常信息
        /// </summary>
        public string? TagErrMsg { get; set; }
        /// <summary>
        /// 变化时间
        /// </summary>
        public DateTime ChangeTime { get; set; }

        /// <summary>
        /// 旧值变化时间
        /// </summary>
        public DateTime OldChangeTime { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public virtual DateTime Timestamp { get; set; }
        /// <summary>
        /// 质量戳
        /// </summary>
        public virtual QualityTypeEnum Quality { get; set; }
        #region 事件委托
        /// <summary>
        /// 值变化委托
        /// </summary>
        public delegate void ValueChangeDelegate(Tag tag);
        /// <summary>
        /// 值变化事件
        /// </summary>
        public event ValueChangeDelegate? ValueChangeEvent;
        #endregion
        public void SendValueChangeEvent()
        {
            ValueChangeEvent?.Invoke(this);
        }
    }
}
