using IIOTS.Util.Infuxdb2;

namespace IIOTS.WebRMS.Models 
{ 
    public class EquStatus
    {
        /// <summary>
        /// 设备状态
        /// </summary>
        [ColumnType(ColumnType.Field), ColumnName("status")]
        public int? status { get; set; }
        /// <summary>
        /// 设备号
        /// </summary> 
        [ColumnType(ColumnType.Tag), ColumnName("equ")]
        public string? equ { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        [ColumnType(ColumnType.Timestamp)]
        public DateTime? _time { get; set; }
    }
}
