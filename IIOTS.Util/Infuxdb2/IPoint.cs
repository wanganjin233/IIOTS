﻿namespace IIOTS.Util.Infuxdb2
{
    /// <summary>
    /// 数据点
    /// </summary>
    public interface IPoint
    {
        /// <summary>
        /// 写入行文本协议内容
        /// </summary>
        /// <param name="writer">写入器 </param>
        void WriteTo(ILineProtocolWriter writer);
    }
}
