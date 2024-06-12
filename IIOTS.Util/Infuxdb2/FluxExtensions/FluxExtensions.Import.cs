namespace IIOTS.Util.Infuxdb2
{
    /// <summary>
    /// Flux扩展
    /// </summary>
    public static partial class FluxExtensions
    {
        /// <summary>
        /// 过滤结果
        /// </summary>
        /// <param name="flux"></param>
        /// <param name="ImportName">引入名</param> 
        /// <returns></returns>
        public static IFlux Import(this IFlux flux, string ImportName)
        {
            return flux.Import(ImportName);
        }
    }
}
