using IIOTS.Util.Infuxdb2;

namespace IIOTS.WebRMS.Models 
{ 
    public class EquStatus
    {
        /// <summary>
        /// 点位名称
        /// </summary>
        [ColumnType(ColumnType.Field), ColumnName("status")]
        public int? status { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        [ColumnType(ColumnType.Field), ColumnName("isActive")]
        public int? isActive { get; set; }
        [ColumnType(ColumnType.Field), ColumnName("equ")]
        public string? equ { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        [ColumnType(ColumnType.Timestamp)]
        public DateTime? _time { get; set; }
    }
}
