﻿namespace IIOTS.Util.Infuxdb2
{
    /// <summary>
    /// Flux扩展
    /// </summary>
    public static partial class FluxExtensions
    {
        /// <summary>
        /// limit
        /// </summary>
        /// <param name="flux"></param>
        /// <param name="n"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static IFlux Limit(this IFlux flux, int n, int offset = 0)
        {
            return flux.Pipe($"limit(n: {n}, offset: {offset})", SingleQuotesBehavior.NoReplace);
        }
    }
}
