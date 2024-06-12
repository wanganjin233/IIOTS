﻿namespace IIOTS.Util.Infuxdb2
{
    /// <summary>
    /// 单引号处理方式
    /// </summary>
    public enum SingleQuotesBehavior
    {
        /// <summary>
        /// 替换为双引号
        /// </summary>
        Replace,

        /// <summary>
        /// 不替换单引号为双引号
        /// </summary>
        NoReplace
    }
}
