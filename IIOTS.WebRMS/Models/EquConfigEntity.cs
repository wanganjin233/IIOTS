using FreeSql.DataAnnotations; 

namespace IIOTS.WebRMS.Models
{
    [Table(Name = "EquConfig")]
    public class EquConfigEntity
    {
        [Column(Name = "ID", IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        /// <summary>
        /// 设备号
        /// </summary>
        [Column(Name = "EQU", IsNullable = false)]
        public string EQU { get; set; } = string.Empty;
        /// <summary>
        /// 连接字符串
        /// </summary>
        [Column(Name = "ConnectionString", IsNullable = false)]
        public string ConnectionString { get; set; } = string.Empty;
        /// <summary>
        /// 扫描周期
        /// </summary>
        [Column(Name = "ScanRate", IsNullable = false)]
        public int ScanRate { get; set; } = 100;
        /// <summary>
        /// 备注
        /// </summary>
        [Column(Name = "Description", IsNullable = true)]
        public string? Description { get; set; }
        /// <summary>
        /// 点位组ID
        /// </summary>
        [Column(Name = "TagGroupId", IsNullable = false)]
        public long TagGroupId { get; set; }
        /// <summary>
        /// 启用
        /// </summary>
        [Column(Name = "IsUse")]
        public bool IsUse { get; set; }
        [Navigate(nameof(TagGroupId))]
        public TagGroupEntity TagGroupEntity { get; set; } = new();
        /// <summary>
        /// 进程ID
        /// </summary>
        [Column(Name = "ProgressId", IsNullable = false)]
        public long ProgressId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreationDate", ServerTime = DateTimeKind.Local, IsNullable = false)]
        public DateTime? CreationDate { get; set; }
    }
}
