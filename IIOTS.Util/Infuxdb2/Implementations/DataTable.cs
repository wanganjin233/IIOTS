﻿using System.Collections; 
using System.Diagnostics;

namespace IIOTS.Util.Infuxdb2.Implementations
{
    /// <summary>
    /// 数据表
    /// </summary> 
    sealed class DataTable : IDataTable, IEnumerable<IDataRow>
    {
        /// <summary>
        /// 获取所有数据行
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IList<IDataRow> rows = new List<IDataRow>();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly TableType tableType;

        /// <summary>
        /// 获取数据行的数量
        /// </summary>
        public int Count => this.rows.Count;

        /// <summary>
        /// 获取表格类型
        /// </summary>
        public TableType TableType => this.tableType;

        /// <summary>
        /// 获取列的集合
        /// </summary>
        public IList<string> Columns { get; }


        /// <summary>
        /// 通过行索引获取数据行
        /// </summary>
        /// <param name="rowIndex">行索引</param>
        /// <returns></returns>
        public IDataRow this[int rowIndex] => this.rows[rowIndex];

        /// <summary>
        /// 数据表
        /// </summary>
        /// <param name="columns">列集合</param>
        public DataTable(IList<string> columns)
        {
            this.Columns = columns;
            if (columns.Count > 1)
            {
                var typeValue = columns[1];
                Enum.TryParse(typeValue, ignoreCase: true, out this.tableType);
            }
        }

        /// <summary>
        /// 添加一行
        /// </summary>
        /// <param name="dataRow"></param>
        public void AddDataRow(IDataRow dataRow)
        {
            this.rows.Add(dataRow);
        }

        /// <summary>
        /// 获取迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IDataRow> GetEnumerator()
        {
            return this.rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
