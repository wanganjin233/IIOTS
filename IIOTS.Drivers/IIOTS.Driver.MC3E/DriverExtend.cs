using IIOTS.Util;

namespace IIOTS.Driver
{
    public static class DriverExtend
    {
        /// <summary>
        /// 校验数据并获取包
        /// </summary>
        /// <param name="_byte"></param>
        /// <returns></returns>
        public static byte[]? GetBody(this byte[]? _byte, bool isBit, int length)
        {
            if (_byte != null && _byte.Length != 0 && _byte.Take(2).Equalsbytes([0, 0]))
            {
                _byte = _byte.Skip(2).ToArray();
                if (isBit)
                {
                    byte[] result = new byte[length];
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = (byte)(_byte[i / 2] >> (i % 2 == 0 ? 4 : 0) & 1);
                    }
                    return result;
                }
                return _byte;
            }
            else
            {
                return null;
            }

        }
    }
}
