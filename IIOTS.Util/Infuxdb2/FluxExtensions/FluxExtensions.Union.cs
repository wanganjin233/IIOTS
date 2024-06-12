namespace IIOTS.Util.Infuxdb2
{
    /// <summary>
    /// Flux扩展
    /// </summary>
    public static partial class FluxExtensions
    {
        /// <summary>
        /// 合并两个查询
        /// </summary>
        /// <param name="flux"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static IFlux Union(this IFlux flux, IFlux other)
        {
            return Flux.Parse($"union(tables:[{flux}, {other}])");
        }
    }
}
