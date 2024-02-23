using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IIOTS.Util
{
    /// <summary>
    /// 拓展类
    /// </summary>
    public static class JsonExtension
    {
        static JsonExtension()
        {
            JsonConvert.DefaultSettings = () => DefaultJsonSetting;
        }
        public static JsonSerializerSettings DefaultJsonSetting = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver(),
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
            DateFormatString = "yyyy-MM-dd HH:mm:ss.fff"
        };

        /// <summary>
        /// 将对象序列化成Json字符串
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        /// <returns></returns>
        public static string ToJson(this object? obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// 将Json字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="jsonStr">Json字符串</param>
        /// <returns></returns>
        public static T? ToObject<T>(this string? jsonStr)
        { 
            return jsonStr == null ? default : JsonConvert.DeserializeObject<T>(jsonStr);
        }
        /// <summary>
        /// 将Json字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="jsonStr">Json字符串</param>
        /// <param name="type">转换类型</param>
        /// <returns></returns>
        public static object? ToObject(this string jsonStr, Type type)
        {
            if (type == typeof(string))
            {
                return jsonStr;
            }
            return new JsonSerializer().Deserialize(new JsonTextReader(new StringReader(jsonStr)), type);
        }
        /// <summary>
        ///  尝试将Json字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="jsonStr">Json字符串</param>
        /// <param name="result">结果</param>
        /// <returns></returns>
        public static bool TryToObject<T>(this string jsonStr, out T? result)
        {
            try
            {
                result = JsonConvert.DeserializeObject<T>(jsonStr);
                return !result.IsNullOrEmpty();
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }
    }
}
