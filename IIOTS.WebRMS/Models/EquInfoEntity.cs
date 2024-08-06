using FreeSql.DataAnnotations; 

namespace IIOTS.WebRMS.Models
{
    [Table(Name = "EquInfo")]
    public class EquInfoEntity
    {
        [Column(Name = "ID", IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        /// <summary>
        /// 工厂名
        /// </summary>
        [Column(Name = "FACTORYNAME")]
        public string FACTORYNAME { get; set; } = string.Empty;
        /// <summary>
        /// 设备名称
        /// </summary>
        [Column(Name = "DEVICENAME")]
        public string DEVICENAME { get; set; } = string.Empty;
        /// <summary>
        /// 设备编码
        /// </summary>
        [Column(Name = "DEVICECODE", IsNullable = false)]
        public string DEVICECODE { get; set; } = string.Empty;
        /// <summary>
        /// 工序编码
        /// </summary>
        [Column(Name = "STEPNAME")]
        public string? STEPNAME { get; set; } 

        public EfficencyData[]? EfficencyDatas { get; set; }
    }
}
