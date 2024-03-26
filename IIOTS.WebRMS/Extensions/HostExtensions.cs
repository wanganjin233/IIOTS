using FreeRedis;
using IIOTS.Util;
using Microsoft.Extensions.Caching.Distributed;

namespace IIOTS.WebRMS
{
    public static class HostExtensions
    {
        /// <summary>
        /// 使用IdHelper
        /// </summary>
        /// <param name="hostBuilder">建造者</param>
        /// <returns></returns>
        public static IHostApplicationBuilder UseIdHelper(this IHostApplicationBuilder hostBuilder)
        {
            new IdHelperBootstrapper()
                  //设置WorkerId
                  .SetWorkderId(hostBuilder.Configuration["WorkerId"]?.ToLong() ?? throw new Exception("机器ID无效"))
                  .Boot();
            return hostBuilder;
        }

        /// <summary>
        /// 使用缓存
        /// </summary>
        /// <param name="hostBuilder">建造者</param>
        /// <returns></returns>
        public static IHostApplicationBuilder UseCache(this IHostApplicationBuilder hostBuilder)
        { 
            var cacheOption = hostBuilder.Configuration.GetSection("Cache").Get<CacheOptions>();
            switch (cacheOption?.CacheType)
            {
                case CacheType.Memory: hostBuilder.Services.AddDistributedMemoryCache(); break;
                case CacheType.Redis:
                    {
                        var redis = new RedisClient(cacheOption.RedisEndpoint);
                        hostBuilder.Services.AddSingleton(redis);
                        hostBuilder.Services.AddSingleton<IDistributedCache>(new DistributedCache(redis));
                    }; break;
                default: throw new Exception("缓存类型无效");
            }
            return hostBuilder;

        }
    }
}
