using FreeSql.DataAnnotations; 

namespace IIOTS.WebRMS.Models
{
    [Table(Name = "ProgressConfig")]
    public class ProgressConfigEntity
    {
        [Column(Name = "ID", IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary> 
        [Column(Name = "Name", IsNullable = false)]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 进程类型
        /// </summary>
        [Column(Name = "ClientType", IsNullable = false)]
        public string ClientType { get; set; } = string.Empty;
        /// <summary>
        /// 组Name
        /// </summary>
        [Column(Name = "Gname", IsNullable = false)]
        public string Gname { get; set; } = string.Empty;

        /// <summary>
        /// 启用
        /// </summary>
        [Column(Name = "IsUse")]
        public bool IsUse { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreationDate", ServerTime = DateTimeKind.Local, IsNullable = false)]
        public DateTime? CreationDate { get; set; }

        [Navigate(nameof(EquConfigEntity.ProgressId))]
        public List<EquConfigEntity> EquConfigEntitys { get; set; } = [];
    }
}
