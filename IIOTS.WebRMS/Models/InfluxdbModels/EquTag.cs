using IIOTS.Util.Infuxdb2;

namespace IIOTS.WebRMS.Models
{
    public class EquTag
    {
        /// <summary>
        /// 点位名称
        /// </summary>
        [ColumnType(ColumnType.Tag),ColumnName("tagName")]
        public string? tagName { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        [ColumnType(ColumnType.Field), ColumnName("Value")]
        public string? Value { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [ColumnType(ColumnType.Tag), ColumnName("Description")]
        public string? Description { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        [ColumnType(ColumnType.Timestamp)]
        public DateTimeOffset? _time { get; set; }
    }
}
