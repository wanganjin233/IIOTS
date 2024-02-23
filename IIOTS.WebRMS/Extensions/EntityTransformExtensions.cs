using IIOTS.Models;
using IIOTS.WebRMS.Models;

namespace IIOTS.WebRMS.Extensions
{
    public static class EntityTransformExtensions
    {
        /// <summary>
        /// 数据库实体转换驱动Tag
        /// </summary>
        /// <param name="tagConfig"></param>
        /// <returns></returns>
        public static TagConfig ToTag(this TagConfigEntity tagConfig)
        {
            return new TagConfig
            {
                TagName = tagConfig.TagName,
                ClientAccess = tagConfig.ClientAccess,
                Address = tagConfig.Address,
                DataType = tagConfig.DataType,
                Coding = tagConfig.Coding,
                Sort = tagConfig.Sort,
                StationNumber = (byte)tagConfig.StationNumber,
                Description = tagConfig.Description,
                EngUnits = tagConfig.EngUnits,
                DataLength = tagConfig.DataLength,
                UpdateMode = tagConfig.UpdateMode,
                Magnification = tagConfig.Magnification
            };
        }
        /// <summary>
        /// 数据库实体转换驱动Tag
        /// </summary>
        /// <param name="tagConfig"></param>
        /// <returns></returns>
        public static List<TagConfig> ToTag(this List<TagConfigEntity> tagConfigs)
        {
            return tagConfigs.Select(p => new TagConfig
            {
                TagName = p.TagName,
                ClientAccess = p.ClientAccess,
                Address = p.Address,
                DataType = p.DataType,
                Coding = p.Coding,
                Sort = p.Sort,
                StationNumber = (byte)p.StationNumber,
                Description = p.Description,
                EngUnits = p.EngUnits,
                DataLength = p.DataLength,
                UpdateMode = p.UpdateMode,
                Magnification = p.Magnification
            }).ToList();
        }

    }
}
