using FreeSql.DataAnnotations; 

namespace IIOTS.WebRMS.Models
{
    [Table(Name = "Users")]
    public class UsersEntity
    {
        [Column(Name = "ID", IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        [Column(Name = "UserName", IsNullable = false)]
        public string UserName { get; set; } = string.Empty;
        /// <summary>
        /// 密码
        /// </summary>
        [Column(Name = "Password", IsNullable = false)]
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// 用户权限等级
        /// </summary>
        [Column(Name = "PrivilegeLevel", IsNullable = false)]
        public string? PrivilegeLevel { get; set; }  
        /// <summary>
        /// 备注
        /// </summary>
        [Column(Name = "Description", IsNullable = true)]
        public string? Description { get; set; } 
        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreationDate", ServerTime = DateTimeKind.Local, IsNullable = false)]
        public DateTime? CreationDate { get; set; }
    }
}
