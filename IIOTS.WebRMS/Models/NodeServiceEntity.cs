using FreeSql.DataAnnotations; 
namespace IIOTS.WebRMS.Models
{
    [Table(Name = "ServiceList")]
    public class NodeServiceEntity
    {
        [Column(Name = "ID", IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        /// <summary>
        /// 服务名称
        /// </summary> 
        [Column(Name = "ServiceName", IsNullable = false)]
        public string ServiceName { get; set; } = string.Empty;
        /// <summary>
        /// 服务节点
        /// </summary> 
        [Column(Name = "ServiceNode", IsNullable = false)]
        public string ServiceNode { get; set; } = string.Empty;
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreationDate", ServerTime = DateTimeKind.Local, IsNullable = false)]
        public DateTime? CreationDate { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Column(Name = "Description", IsNullable = true)]
        public string? Description { get; set; }

    }
}
