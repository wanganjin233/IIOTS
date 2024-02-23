using IIOTS.Util;

namespace IIOTS.Driver
{
    public static class DriverExtend
    {
        //46 49 4E 53 00 00 00 18 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 01 00 00 00 00
        //46 49 4E 53 00 00 00 10 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 01 
        /// <summary>
        /// 登录成功标识
        /// </summary>
        private static readonly byte[] LoginSuccess = new byte[4] { 0, 0, 0, 0 };
        /// <summary>
        /// 读取成功标识
        /// </summary>
        private static readonly byte[] ReadSuccess = new byte[2] { 0, 0 };
        /// <summary>
        /// 校验数据并获取包
        /// </summary>
        /// <param name="_byte"></param>
        /// <returns></returns>
        public static byte[]? GetLogIn(this byte[]? _byte)
        {
            if (_byte != null && _byte.Equalsbytes(LoginSuccess, 4))
            {
                return _byte.Skip(8).ToArray();
            }
            return null;
        }
        /// <summary>
        /// 获取包内容
        /// </summary>
        /// <param name="_byte"></param>
        /// <returns></returns>
        public static byte[]? GetBody(this byte[]? _byte)
        {
            if (_byte != null && _byte.Length >= 22 && _byte.Equalsbytes(ReadSuccess, 20))
            {
                _byte = _byte.Skip(22).ToArray();
                return _byte;
            }
            return null;
        }
    }
}
