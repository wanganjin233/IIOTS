namespace IIOTS.Models
{
    public class ProgressConfig
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 备注
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// 处理类
        /// </summary>
        public List<string> Operations { get; set; } = [];
        /// <summary>
        /// 设备配置组
        /// </summary>
        public List<EquConfig> EquConfigs { get; set; } = [];


    }
}
