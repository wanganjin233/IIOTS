using IIOTS.Enums;

namespace IIOTS.Models
{
    public class TagGroup
    {
        /// <summary>
        /// 组命令
        /// </summary>
        public byte[]? Command { get; set; }
        /// <summary>
        /// 开始地址
        /// </summary>
        public int StartAddress { get; set; }
        /// <summary>
        /// 长度
        /// </summary>
        public ushort Length { get; set; } 
        /// <summary>
        /// 是否为位
        /// </summary>
        public bool IsBit { get; set; } = false;
        /// <summary>
        /// 更新点位方式
        /// </summary>
        public UpdateModeEnum UpdateMode { get; set; } = UpdateModeEnum.Poll;
        /// <summary>
        /// tag点
        /// </summary>
        public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}
