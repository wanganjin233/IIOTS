using IIOTS.Util;
using IIOTS.WebRMS.Models;
using Microsoft.AspNetCore.Components;
using AntDesign;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IIOTS.WebRMS.Component
{
    [Route("/api/Auth/getEapSsoUrl")]
    public partial class AuthComponent : ComponentBase
    {
        [Inject]
        private NotificationService _notice { get; set; } = default!;
        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Inject]
        private IConfiguration _configuration { get; set; } = default!;
        [Inject]
        private IFreeSql FreeSql { get; set; } = default!;
        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;
        /// <summary>
        /// 获取单点登录token
        /// </summary>
        /// <param name="code"></param>
        /// <param name="maxkeyClientId"></param>
        /// <param name="maxkeyClientSecret"></param>
        /// <param name="redirectUri"></param>
        /// <returns></returns>
        private async Task<string?> getMaxkeyToken(string code, string maxkeyClientId, string maxkeyClientSecret, string redirectUri)
        {
            string? token = null;
            string? maxkeyGetTokenUrl = _configuration.GetValue<string>("Maxkey:maxkeyGetTokenUrl");
            using var client = new HttpClient();
            string url = maxkeyGetTokenUrl + "?client_id=" + maxkeyClientId + "&client_secret=" + maxkeyClientSecret + "&grant_type=authorization_code" + "&redirect_uri=" + redirectUri + "&code=" + code;

            var response = await client.GetAsync(url);
            // 检查是否请求成功
            if (response.IsSuccessStatusCode)
            {
                // 读取响应内容
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject? jObjectBody = JsonConvert.DeserializeObject<JObject>(responseBody);
                if (jObjectBody != null)
                {
                    token = jObjectBody["access_token"]?.ToString();
                }
            }
            return token;
        }
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<string?> getMaxkeyUserInfo(string token)
        {
            string? workCode = null;
            string? maxKeyGetUserInfoUrl = _configuration.GetValue<string>("Maxkey:maxKeyGetUserInfoUrl");
            using var client = new HttpClient();
            string url = maxKeyGetUserInfoUrl + "?access_token=" + token;
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject? jObjectBody = JsonConvert.DeserializeObject<JObject>(responseBody);
                if (jObjectBody != null)
                {
                    workCode = jObjectBody["employeeNumber"]?.ToString();
                }
            }
            return workCode;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("code", out var code))
            {
                if (!string.IsNullOrEmpty(code))
                {
                    string ssoUrl = string.Empty;
                    string? maxkeyClientId = _configuration.GetValue<string>("Maxkey:maxkeyClientId");
                    string? maxkeyClientSecret = _configuration.GetValue<string>("Maxkey:maxkeyClientSecret");
                    string? redirectUri = _configuration.GetValue<string>("Maxkey:redirectUri");
                    if (!string.IsNullOrEmpty(maxkeyClientId) && !string.IsNullOrEmpty(maxkeyClientSecret) && !string.IsNullOrEmpty(redirectUri))
                    {
                        string? token = await getMaxkeyToken(code, maxkeyClientId, maxkeyClientSecret, redirectUri);
                        if (token != null)
                        {
                            string? workCode = await getMaxkeyUserInfo(token);
                            if (workCode != null)
                            {
                                var Users = await FreeSql
                                .Select<UsersEntity>()
                                .Where(p => p.UserName == workCode)
                                .FirstAsync();
                                if (Users != null)
                                {
                                    UserDto userDto = new()
                                    {
                                        Claims = new Dictionary<string, string>
                                    {
                                        {
                                            ClaimTypes.Name,
                                            workCode
                                        },
                                        {
                                            Users.PrivilegeLevel,
                                            Users.PrivilegeLevel
                                        }
                                    }
                                    };
                                    JWTTokenOptions? jWTTokenOptions = _configuration.GetSection("JWTTokenOptions").Get<JWTTokenOptions>();
                                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jWTTokenOptions.SecurityKey));
                                    var expires = DateTime.Now.AddDays(7);
                                    userDto.Token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
                                        issuer: jWTTokenOptions.Issuer,
                                        audience: jWTTokenOptions.Audience,
                                        claims: userDto.Claims.Select(p => new Claim(p.Key, p.Value)),
                                        notBefore: DateTime.Now,
                                        expires: expires,
                                        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)));
                                    ((AuthProvider)AuthenticationStateProvider).MarkUserAsAuthenticated(userDto);
                                    NavigationManager.NavigateTo("/", false, true);
                                }
                                else
                                {
                                    await _notice.Error(new NotificationConfig()
                                    {
                                        Message = "登录失败",
                                        Description = "无访问权限"
                                    });
                                }
                            }
                        }
                    }
                }
            }
            await base.OnInitializedAsync();
        }
    }
}
