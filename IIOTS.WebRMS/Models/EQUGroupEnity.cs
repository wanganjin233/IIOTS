using AntDesign.Charts;
using FreeSql.DataAnnotations;

namespace IIOTS.WebRMS.Models
{
    [Table(Name = "IIOTS.EQUGroup")]
    public class EQUGroupEnity
    {
        [Column(Name = "ID", IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        /// <summary>
        /// 设备组
        /// </summary>
        [Column(Name = "EQU", IsNullable = false)]
        public string EQU { get; set; }
        /// <summary>
        /// 工厂
        /// </summary>
        [Column(Name = "Factory", IsNullable = false)]
        public string Factory { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Column(Name = "Description")]
        public string? Description { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreatTime", ServerTime = DateTimeKind.Local, IsNullable = false)]
        public DateTime? CreatTime { get; set; }
    }
}
