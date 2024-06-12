using IIOTS.Util.Infuxdb2;

namespace IIOTS.WebRMS.Models
{
    public class EquAlarm
    {
        /// <summary>
        /// 点位名称
        /// </summary>
        [ColumnType(ColumnType.Field),ColumnName("enable")]
        public bool? enable { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        [ColumnType(ColumnType.Field), ColumnName("content")]
        public string? content { get; set; }
        [ColumnType(ColumnType.Field), ColumnName("equ")]
        public string? equ { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        [ColumnType(ColumnType.Timestamp)]
        public DateTime? _time { get; set; }
    }
}
