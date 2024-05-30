using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IIOTS.WebRMS.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace IIOTS.WebRMS.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AuthController(IConfiguration _configuration, IFreeSql FreeSql) : ControllerBase
    {
        private readonly JWTTokenOptions? jWTTokenOptions = _configuration.GetSection("JWTTokenOptions").Get<JWTTokenOptions>();
        //获得用户信息
        [HttpGet]
        public Dictionary<string, string> GetUser()
        {
            if (User.Identity?.IsAuthenticated ?? false)//如果Token有效
            {
                return User.Claims.ToDictionary(p => p.Type, a => a.Value);
            }
            else
            {
                return [];
            }
        }
        //登录
        [HttpPost]
        public UserDto Login(LoginDto loginDto)
        {
            var Users = FreeSql
                        .Select<UsersEntity>()
                        .Where(p => p.UserName == loginDto.UserName && p.Password == loginDto.Password)
                        .First();
            if (Users != null)
            {
                UserDto userDto = new()
                {
                    Claims = new Dictionary<string, string>
                    {
                        {ClaimTypes.Name, loginDto.UserName },
                        {Users.PrivilegeLevel, Users.PrivilegeLevel }
                    }
                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jWTTokenOptions.SecurityKey));
                var expires = DateTime.Now.AddDays(7);
                var token = new JwtSecurityToken(
                    issuer: jWTTokenOptions.Issuer,
                    audience: jWTTokenOptions.Audience,
                    claims: userDto.Claims.Select(p => new Claim(p.Key, p.Value)),
                    notBefore: DateTime.Now,
                    expires: expires,
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
                userDto.Token = new JwtSecurityTokenHandler().WriteToken(token);
                return userDto;
            }
            else
            {
                return new UserDto();

            }
        }
    }
}
