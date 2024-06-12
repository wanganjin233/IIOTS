using IIOTS.Enums; 

namespace IIOTS.Models
{
    public class EquConfig
    {
        /// <summary>
        /// 设备号
        /// </summary>
        public string EQU { get; set; } = string.Empty;
        /// <summary>
        /// 驱动类型
        /// </summary>
        public DriverTypeEnum DriverType { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;
        /// <summary>
        /// 扫描周期
        /// </summary>
        public int ScanRate { get; set; } = 100;
        /// <summary>
        /// 备注
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// 点位列表
        /// </summary>
        public List<TagConfig> Tags { get; set; } = new List<TagConfig>();
        /// <summary>
        /// 点位配置路径
        /// </summary>
        public string? TagConfigPath { get; set; }
    }

}
