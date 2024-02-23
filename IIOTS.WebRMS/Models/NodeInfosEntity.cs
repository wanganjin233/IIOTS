using FreeSql.DataAnnotations;
using AntDesign.Charts;


namespace IIOTS.WebRMS.Models
{
    [Table(Name = "NodeInfos")]
    public class NodeInfosEntity
    {
        [Column(Name = "ID", IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        /// <summary>
        /// 节点名称
        /// </summary>
        [Column(Name = "EdgeID", IsNullable = false)]
        public string EdgeID { get; set; }
        /// <summary>
        /// 调试模式
        /// </summary>
        [Column(Name = "LocalMode") ]
        public string? LocalMode { get; set; }
        /// <summary>
        /// IP
        /// </summary>
        [Column(Name = "IPNo", IsNullable = false)]
        public string IPNo { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateTime", IsNullable = false)]
        public string CreateTime { get; set; }
        /// <summary>
        /// 最后一次启动时间
        /// </summary>
        [Column(Name = "LastStartTime")]
        public string? LastStartTime { get; set; }
    }
}
