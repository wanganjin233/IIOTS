namespace IIOTS.Driver
{
    internal static class DriverExtend
    {
        /// <summary>
        /// 校验数据并获取包
        /// </summary>
        /// <param name="_byte">完整报文数据</param>
        /// <param name="Length">长度</param>
        /// <returns></returns>
        internal static byte[]? GetBody(this byte[]? _byte, bool isBit = false, int Length = 0)
        {
            if (_byte != null)
            {
                _byte = _byte.Skip(3).ToArray(); //截取内容
                if (isBit)//读取布尔类型长度
                {
                    byte[] result = new byte[Length];
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = (byte)(_byte[i / 8] >> (byte)(i % 8) & 1);
                    }
                    return result;
                }
                else
                {
                    return _byte;
                }
            }
            return null;
        }
        /// <summary>
        /// 校验写入数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        internal static bool Verify(this byte[]? bytes)
        {
            if (bytes != null && bytes.Length > 9)
            {
                return true;
            }
            return false;
        }
    }
}