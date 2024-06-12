﻿using System.Collections;
using System.Collections.Generic;

namespace IIOTS.Util.Infuxdb2
{
    /// <summary>
    /// 数据表
    /// </summary>
    public interface IDataTable : IEnumerable
    {
        /// <summary>
        /// 获取数据行的数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 获取表格类型
        /// </summary>
        TableType TableType { get; }

        /// <summary>
        /// 获取列的集合
        /// </summary>
        IList<string> Columns { get; }

        /// <summary>
        /// 通过行索引获取数据行
        /// </summary>
        /// <param name="rowIndex">行索引</param>
        /// <returns></returns>
        IDataRow this[int rowIndex] { get; }
    }
}
