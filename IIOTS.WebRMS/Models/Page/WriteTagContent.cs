using IIOTS.Enums;
namespace IIOTS.WebRMS.Models
{
    public class WriteTagContent
    {
        /// <summary>
        /// 点位名
        /// </summary>
        public string TagName { get; set; } = string.Empty;
        /// <summary>
        /// 值类型
        /// </summary>
        public TagTypeEnum Type { get; set; }
        /// <summary>
        /// 写入值
        /// </summary>
        public object? WriteValue { get; set; }
    }
}
