using FreeSql.DataAnnotations;
using IIOTS.Enum;

namespace IIOTS.WebRMS.Models
{
    [Table(Name = "TagGroup")]
    public class TagGroupEntity
    {
        [Column(Name = "ID", IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        /// <summary>
        /// 组名
        /// </summary>
        [Column(Name = "TagGName", IsNullable = false)]
        public string TagGName { get; set; } = string.Empty;
        /// <summary>
        /// 备注
        /// </summary>
        [Column(Name = "Description")]
        public string? Description { get; set; }
        /// <summary>
        /// 流程ID
        /// </summary>
        [Column(Name = "FlowId")]
        public string? FlowId { get; set; }
        /// <summary>
        /// 驱动类型
        /// </summary> 
        [Column(Name = "DriverType", IsNullable = false, MapType = typeof(string))]
        public DriverTypeEnum DriverType { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreationDate", ServerTime = DateTimeKind.Local, IsNullable = false)]
        public DateTime? CreationDate { get; set; }
        [Navigate(nameof(TagConfigEntity.GID))]
        public List<TagConfigEntity> TagConfigEntitys { get; set; } = new();

    }
}
