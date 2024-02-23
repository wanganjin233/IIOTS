using IIOTS.Util;

namespace IIOTS.Driver
{
    public static class DriverExtend
    {
        /// <summary>
        /// 校验数据并获取包
        /// </summary>
        /// <param name="_byte">完整报文数据</param>
        /// <param name="Length">长度</param>
        /// <returns></returns>
        public static byte[]? GetBody(this byte[]? _byte, bool isBit = false, int Length = 0)
        {
            if (_byte != null
                && _byte.Length >= 6 //最小长度
                && _byte.CRC16Verify() //校验CRC16 
                )
            {
                _byte = _byte.Skip(3).Take(_byte.Length - 5).ToArray(); //截取内容
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
        /// 校验写入成功报文
        /// </summary>
        /// <param name="data"></param>
        /// <param name="comm"></param>
        /// <returns></returns>
        public static bool WriteVerify(this byte[]? data, byte[] comm)
        {
            if (data.CRC16Verify())
            {
                comm = comm.Take(data.Length - 2).ToArray();
                data = data.Take(data.Length - 2).ToArray();
                return comm.Equalsbytes(data);
            }
            return false;
        }
        /// <summary>
        /// 校验CRC16
        /// </summary>
        /// <param name="dataBuff">完整报文数据</param>
        /// <returns></returns>
        private static bool CRC16Verify(this byte[]? dataBuff)
        {

            if (dataBuff == null || dataBuff.Length < 2) return false;
            int CRCResult = 0xFFFF;
            for (int i = 0; i < dataBuff.Length - 2; i++)
            {
                CRCResult ^= dataBuff[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((CRCResult & 1) == 1)
                        CRCResult = CRCResult >> 1 ^ 0xA001;
                    else CRCResult >>= 1;
                }
            }
            return dataBuff[^1] == Convert.ToByte(CRCResult >> 8)
                 && dataBuff[^2] == Convert.ToByte(CRCResult & 0xff);
        }
    }
}
