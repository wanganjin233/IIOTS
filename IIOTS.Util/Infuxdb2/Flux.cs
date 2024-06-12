﻿using System;
using System.Text;

namespace IIOTS.Util.Infuxdb2
{
    /// <summary>
    /// 提供IFlux的创建
    /// </summary>
    public static class Flux
    {
        /// <summary>
        /// From语句
        /// </summary>
        /// <param name="bucket"></param>
        /// <returns></returns>
        public static IFlux From(string bucket)
        {
            return Parse(@$"from(bucket: ""{bucket}"")");
        }

        /// <summary>
        /// 从fluxText转换
        /// </summary>
        /// <param name="fluxText"></param> 
        /// <returns></returns>
        public static IFlux Parse(string fluxText)
        {
            return new FluxImpl(fluxText);
        }

        /// <summary>
        /// Flux实现
        /// </summary>
        private class FluxImpl : IFlux
        {
            private readonly StringBuilder builder = new(); 
            /// <summary>
            /// Flux实现
            /// </summary>
            /// <param name="fluxText"></param> 
            public FluxImpl(string fluxText)
            { 
                this.builder.Append(fluxText);
            }

            /// <summary>
            /// 添加管道
            /// </summary>
            /// <param name="content">值，不包含管道符号 </param>
            /// <param name="behavior">单引号处理方式</param>
            /// <returns></returns>
            public IFlux Pipe(string content, SingleQuotesBehavior behavior)
            {
                if (string.IsNullOrEmpty(content))
                {
                    return this;
                }

                if (behavior == SingleQuotesBehavior.Replace)
                {
                    content = RepaceSingleQuotes(content);
                }

                this.builder.AppendLine().Append("|> ").Append(content);
                return this;
            }   /// <summary>
                /// 添加管道
                /// </summary>
                /// <param name="content">值  </param> 
                /// <returns></returns>
            public IFlux Import(string content )
            {
                if (string.IsNullOrEmpty(content))
                {
                    return this;
                } 
                this.builder.Insert(0, $"import \"{content}\"\n");
                return this;
            }
            /// <summary>
            /// 替换单引号 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            private static string RepaceSingleQuotes(string value)
            {
                var span = value.ToCharArray().AsSpan();
                for (var i = 0; i < span.Length; i++)
                {
                    if (span[i] == '\'')
                    {
                        span[i] = '"';
                    }
                }
                return span.ToString();
            }

            /// <summary>
            /// 转换为文本
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return this.builder.ToString();
            }
        }

    }
}
