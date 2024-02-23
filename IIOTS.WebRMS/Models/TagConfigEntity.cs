using FreeSql.DataAnnotations;
using IIOTS.Enum;

namespace IIOTS.WebRMS.Models
{
    [Table(Name = "TagConfig")]
    public class TagConfigEntity
    {
        [Column(Name = "ID", IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        /// <summary>
        /// 站号
        /// </summary>
        [Column(Name = "StationNumber")]
        public long StationNumber { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        [Column(Name = "DataType", MapType = typeof(string), IsNullable = false)]
        public TagTypeEnum DataType { get; set; } = TagTypeEnum.Short;
        /// <summary>
        /// 点位名称
        /// </summary>
        [Column(Name = "TagName", IsNullable = false)]
        public string TagName { get; set; } = string.Empty;
        /// <summary>
        /// 地址
        /// </summary>
        [Column(Name = "Address", IsNullable = false)]
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// 数据长度
        /// </summary>
        [Column(Name = "DataLength")]
        public int DataLength { get; set; }
        /// <summary>
        /// 缩放
        /// </summary>
        [Column(Name = "Magnification")]
        public double Magnification { get; set; }
        /// <summary>
        /// 读写权限
        /// </summary>
        [Column(Name = "ClientAccess", MapType = typeof(string), IsNullable = false)]
        public ClientAccessEnum ClientAccess { get; set; } = ClientAccessEnum.RW;
        /// <summary>
        /// 单位
        /// </summary>
        [Column(Name = "EngUnits", IsNullable = true)]
        public string? EngUnits { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Column(Name = "Description", IsNullable = true)]
        public string? Description { get; set; }
        /// <summary>
        /// 顺序
        /// </summary>
        [Column(Name = "Sort", MapType = typeof(string), IsNullable = false)]
        public SortEnum Sort { get; set; } = SortEnum.ABCD;
        /// <summary>
        /// 编码
        /// </summary>
        [Column(Name = "Coding", MapType = typeof(string))]
        public CodingEnum Coding { get; set; }
        /// <summary>
        /// 点位更新模式
        /// </summary>
        [Column(Name = "UpdateMode", MapType = typeof(string), IsNullable = false)]
        public UpdateModeEnum UpdateMode { get; set; } = UpdateModeEnum.Sub;
        /// <summary>
        /// 组id
        /// </summary>
        [Column(Name = "GID")]
        public long GID { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreationDate", ServerTime = DateTimeKind.Local, IsNullable = false)]
        public DateTime? CreationDate { get; set; }
    }
}
