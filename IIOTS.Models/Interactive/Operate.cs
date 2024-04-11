using IIOTS.Enum;

namespace IIOTS.Models
{
    public class Operate<T>
    {  
        /// <summary>
        /// 标识
        /// </summary>
        public required string  Id { get; set; } 
        /// <summary>
        /// 用户自定义的泛型数据
        /// </summary>
        public required T  Content { get; set; }
    }
}
