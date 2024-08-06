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
        public static byte[]? GetBody(this byte[]? _byte, int length)
        {
            if (_byte != null && _byte.Length != 0)
            {
                int num = 0;
                for (int i = 1; i < _byte.Length - 2; i++)
                {
                    num += _byte[i];
                }
                if (num == _byte.TakeLast(2).ToArray().ToASCIIString().ToInt0X())
                {
                    _byte = _byte.Skip(1).SkipLast(3).ToArray();
                    if (_byte.Length == length * 2)
                    {
                        return _byte.ToASCIIString().To0XBytes();
                    }
                }
            }
            return null;
        }
    }
}
