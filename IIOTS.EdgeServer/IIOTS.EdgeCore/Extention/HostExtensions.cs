using IIOTS.Util;

namespace IIOTS.EdgeCore.Extension
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
                  .SetWorkderId(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds())
                  .Boot();
            Config.Identifier = "EdgeCore";
            return hostBuilder;
        } 
    }
}
