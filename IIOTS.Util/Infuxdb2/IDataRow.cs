﻿using System.Collections;
using System.Collections.Generic;

namespace IIOTS.Util.Infuxdb2
{
    /// <summary>
    /// 数据行
    /// </summary>
    public interface IDataRow : IEnumerable
    {
        /// <summary>
        /// 获取列集合
        /// </summary>
        IList<string> Columns { get; }

        /// <summary>
        /// 获取值集合
        /// </summary>
        IList<string?> Values { get; }

        /// <summary>
        /// 通过列索引获取值
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        string? this[int columnIndex] { get; }

        /// <summary>
        /// 尝试获取列的数据
        /// </summary>
        /// <param name="column">列名</param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetValue(string column, out string? value);
    }
}
