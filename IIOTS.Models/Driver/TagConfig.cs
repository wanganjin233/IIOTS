using IIOTS.Enum;

namespace IIOTS.Models
{
    public class TagConfig
    {  /// <summary>
       /// 缩放倍数
       /// </summary>
        public double Magnification { get; set; } = 1;
        /// <summary>
        /// 单位
        /// </summary>
        public string? EngUnits { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// 顺序
        /// </summary>
        public SortEnum Sort { get; set; } = SortEnum.ABCD;
        /// <summary>
        /// 编码
        /// </summary>
        public CodingEnum Coding { get; set; } = CodingEnum.ASCII;
        /// <summary>
        /// 点位更新模式
        /// </summary>
        public UpdateModeEnum UpdateMode { get; set; } = UpdateModeEnum.Sub;
        /// <summary>
        /// 变量类型
        /// </summary>
        public TagTypeEnum DataType { get; set; } = TagTypeEnum.Short;
        /// <summary>
        /// 点位名称
        /// </summary>
        public string TagName { get; set; } = string.Empty;
        /// <summary>
        /// 完整地址
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// 读写权限
        /// </summary>
        public ClientAccessEnum ClientAccess { get; set; } = ClientAccessEnum.RW;
        /// <summary>
        /// 站号
        /// </summary>
        public byte StationNumber { get; set; } = 1;
        private int dataLength = 0;
        /// <summary>
        /// 数据长度
        /// </summary>
        public int DataLength
        {
            get
            {
                return DataType switch
                {
                    TagTypeEnum.Boole => 1,
                    TagTypeEnum.Ushort => 2,
                    TagTypeEnum.Short => 2,
                    TagTypeEnum.Uint => 4,
                    TagTypeEnum.Int => 4,
                    TagTypeEnum.Float => 4,
                    TagTypeEnum.Double => 8,
                    TagTypeEnum.Ulong => 8,
                    TagTypeEnum.Long => 8,
                    _ => dataLength
                };
            }
            set
            {
                dataLength = value;
            }
        }

    }
}
