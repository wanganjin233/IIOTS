using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Quartz.Util;
using System.Net.Http.Headers;
using System.Security.Policy;

namespace IIOTS.WebRMS.Extensions
{
    public static class NodeRedApi
    {
        public static IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
        private static string? url;
        private static string? username;
        private static string? password;
        public static void UseNodeRedApi(this IHostApplicationBuilder hostBuilder)
        {
            url = hostBuilder.Configuration["NodeRedUrl"]?.ToString();
            username = hostBuilder.Configuration["username"]?.ToString();
            password = hostBuilder.Configuration["password"]?.ToString();
        } 
        public static async Task<string?> GetTokenAsync()
        {
            if (!_memoryCache.TryGetValue<string>("Token", out string? token))
            {
                using HttpClient client = new();
                //设置参数
                var requestData = new Dictionary<string, string?>
                    {
                     { "client_id", "node-red-admin" },
                     { "grant_type", "password" },
                     { "scope", "*" },
                     { "username", username },
                     { "password",password }
                    };

                var requestContent = new FormUrlEncodedContent(requestData);

                // 发送HTTP POST请求并获取响应
                var response = await client.PostAsync($"{url}/auth/token", requestContent);

                // 检查是否请求成功
                if (response.IsSuccessStatusCode)
                {
                    // 读取响应内容
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var access_token = JsonConvert.DeserializeObject<JObject>(responseBody)["access_token"].ToString();
                    _memoryCache.Set("Token", access_token, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(7)));
                    return access_token;
                }
            }
            else
            {
                return token?.ToString();
            }
            return null;
        }
        public static string GetFlows()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", "application/x-www-form-urlencoded"); 
            HttpResponseMessage response = httpClient.GetAsync($"{url}/flows").Result;
            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                return result;
            }
            return string.Empty;
        }
    }
}
