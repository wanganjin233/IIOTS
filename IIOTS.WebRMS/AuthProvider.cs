﻿using Blazored.LocalStorage;
using IIOTS.WebRMS.Models; 
using Microsoft.AspNetCore.Components.Authorization; 
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; 
using System.Net.Http.Headers;
using System.Security.Claims; 

namespace IIOTS.WebRMS
{ 
    public class AuthProvider(HttpClient httpClient, ILocalStorageService localStorage) : AuthenticationStateProvider
    {
        public AuthenticationState? AuthState { get; set; }


        /// <summary>
        /// 获取验证状态
        /// </summary>
        /// <returns></returns>
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            //获取本地存储token
            if (httpClient.DefaultRequestHeaders.Authorization == null)
            {
                var token = await localStorage.GetItemAsStringAsync("token");
                if (token != null)
                {
                    JObject? jObjectBody = JsonConvert.DeserializeObject<JObject>(token);
                    if (jObjectBody != null)
                    {
                        var access_token = jObjectBody["access_token"]?.ToString();
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", access_token);
                    }
                }
            }
            //获得用户登录状态 
            var claims = await httpClient.GetFromJsonAsync<Dictionary<string, string>>($"api/Auth/GetUser");
            if (claims?.Count == 0)
            {
                MarkUserAsLoggedOut();
                return new AuthenticationState(new ClaimsPrincipal());
            }
            else
            {
                var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims.Select(p => new Claim(p.Key, p.Value)), "iiotsauth"));
                AuthState = new AuthenticationState(authenticatedUser);
                return AuthState;
            }
        }

        /// <summary>
        /// 标记授权
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        public void MarkUserAsAuthenticated(UserDto userDto)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", userDto.Token);
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(userDto.Claims.Select(p => new Claim(p.Key, p.Value)), "iiotsauth"));
            AuthState = new AuthenticationState(authenticatedUser);
            var authState = Task.FromResult(AuthState);
            NotifyAuthenticationStateChanged(authState);
            localStorage.SetItemAsync("token", new
            {
                access_token = userDto.Token,
                expires_in = 604800,
                token_type = "Bearer"
            });
        }

        /// <summary>
        /// 标记注销
        /// </summary>
        public void MarkUserAsLoggedOut()
        {
            httpClient.DefaultRequestHeaders.Authorization = null;
            AuthState = null;

            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }
    }
}
