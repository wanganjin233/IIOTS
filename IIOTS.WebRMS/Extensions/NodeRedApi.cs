using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;

namespace IIOTS.WebRMS
{
    public static class NodeRedApi
    {
        private static IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
        private static string? url;
        public static string? Url => url;
        private static string? username;
        private static string? password;
        /// <summary>
        /// 初始化获取node配置
        /// </summary>
        /// <param name="hostBuilder"></param>
        public static void UseNodeRedApi(this IHostApplicationBuilder hostBuilder)
        {
            url = hostBuilder.Configuration["NodeRed:NodeRedUrl"]?.ToString();
            username = hostBuilder.Configuration["NodeRed:username"]?.ToString();
            password = hostBuilder.Configuration["NodeRed:password"]?.ToString();
        }
        /// <summary>
        /// 获取Token
        /// </summary>
        /// <returns></returns>
        public static async Task<string?> GetTokenAsync()
        {
            if (!_memoryCache.TryGetValue("Token", out string? token))
            {
                using var client = new HttpClient();
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
                    JObject? jObjectBody = JsonConvert.DeserializeObject<JObject>(responseBody);
                    if (jObjectBody != null)
                    {
                        var access_token = jObjectBody["access_token"]?.ToString();

                        //保存Token至缓存
                        _memoryCache.Set("Token", access_token, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(7)));
                        //返回Token值
                        return access_token;
                    }
                }
            }
            else
            {
                return token?.ToString();
            }
            return null;
        }

        /// <summary>
        /// 获取流信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<string?> GetFlowAsync(string id)
        {
            string? token = await GetTokenAsync();
            if (token != null)
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                HttpResponseMessage response = await httpClient.GetAsync($"{url}/flow/{id}");
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取Tab流信息
        /// </summary> >
        /// <returns></returns>
        public static async Task<List<string>?> GetTabFlowIdsAsync()
        {
            string? token = await GetTokenAsync();
            if (token != null)
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                HttpResponseMessage response = await httpClient.GetAsync($"{url}/flows");
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    JArray? JArrayBody = JsonConvert.DeserializeObject<JArray>(result);

                    if (JArrayBody != null)
                    {
                        var flowIds = JArrayBody
                            .ToList()
                            .Where(p => p["type"]?.ToString() == "tab")
                            .Select(item => item["id"]?.ToString() ?? "")
                            .ToList();
                        return flowIds;
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 创建流程
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static async Task<string?> CreateFlowAsync(string label)
        {
            string? token = await GetTokenAsync();
            if (token != null)
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                HttpContent content = new StringContent("{ \"label\": \"" + label + "\",\"nodes\": [ ]}", Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync($"{url}/flow", content);
                if (response.IsSuccessStatusCode)
                {
                    // 读取响应内容
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject? jObjectBody = JsonConvert.DeserializeObject<JObject>(responseBody);
                    if (jObjectBody != null)
                    {
                        var flowId = jObjectBody["id"]?.ToString();
                        //返回Token值
                        return flowId;
                    }
                }
            }
            return null;
        }
    }
}
