using FreeSql.DataAnnotations;
using AntDesign.Charts;

namespace IIOTS.WebRMS.Models
{
    [Table(Name = "IIOTS.EQUGroupSubList")]
    public class EQUGroupSublistEnity
    {
        [Column(Name = "ID", IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        /// <summary>
        /// 设备线
        /// </summary>
        [Column(Name = "EQULine", IsNullable = false)]
        public string EQULine { get; set; }
        /// <summary>
        /// 设备号
        /// </summary>
        [Column(Name = "EQUNo", IsNullable = false)]
        public string EQUNo { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        [Column(Name = "Description", IsNullable = false)]
        public string Description { get; set; }
        /// <summary>
        /// 分组
        /// </summary>
        [Column(Name = "GID", IsNullable = false)]
        public string GID { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreatTime", IsNullable = false)]
        public string CreatTime { get; set; }
 
    }
}
