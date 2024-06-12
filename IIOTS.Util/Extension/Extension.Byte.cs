using IIOTS.Enums;
using System.Text;

namespace IIOTS.Util
{
    /// <summary>
    /// 拓展类
    /// </summary>
    public static partial class Extension
    {
        /// <summary>
        /// byte数组增加
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <returns></returns>
        public static byte[] AddBytes(this byte[] data1, byte[] data2)
        {
            byte[] data3 = new byte[data1.Length + data2.Length];
            data1.CopyTo(data3, 0);
            data2.CopyTo(data3, data1.Length);
            return data3;
        }
        /// <summary>
        /// 查找byte位置
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int IndexOf(this IEnumerable<byte> bytes, IEnumerable<byte> value)
        {
            for (int i = 0; i <= bytes.Count() - value.Count(); i++)
            {
                if (bytes.Equalsbytes(value, i))
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// 查找byte位置
        /// </summary>
        /// <param name="byte"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int IndexOf(this IEnumerable<byte> bytes, byte value)
        {
            for (int i = 0; i < bytes.Count(); i++)
            {
                if (bytes.ElementAt(i) == value)
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// 从后查找byte位置
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int LastIndexOf(this IEnumerable<byte> bytes, IEnumerable<byte> value)
        {
            int index = bytes.Reverse().IndexOf(value.Reverse());
            if (index != -1)
            {
                index = bytes.Count() - index - value.Count();
            }
            return index;
        }
        /// <summary>
        /// 对比两组bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Equalsbytes(this IEnumerable<byte> bytes, IEnumerable<byte>? value, int startIndex = 0)
        {
            if (value == null || bytes.Count() < startIndex + value.Count()) return false;
            for (int i = 0; i < value.Count(); i++)
            {
                if (value.ElementAt(i) != bytes.ElementAt(i + startIndex))
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 切分bytes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_byte"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static List<byte[]> Split(this IEnumerable<byte> bytes, IEnumerable<byte> separator)
        {
            List<byte[]> bytesList = new List<byte[]>();
            do
            {
                int index = bytes.IndexOf(separator);
                if (index > -1)
                {
                    byte[] data = bytes.Take(index).ToArray();
                    bytesList.Add(data);
                    bytes = bytes.Skip(index + separator.Count());
                }
                else
                {
                    bytesList.Add(bytes.ToArray());
                    break;
                }
            } while (true);
            return bytesList;
        }
        /// <summary>
        /// 根据头尾字节切分bytes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_byte"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static List<byte[]> Capture(this IEnumerable<byte> bytes, IEnumerable<byte> startBytes, IEnumerable<byte> endBytes)
        {
            List<byte[]> bytesList = new List<byte[]>();
            do
            {
                int startIndex = bytes.IndexOf(startBytes);
                int endIndex = bytes.IndexOf(endBytes);
                int length = endIndex - startIndex - startBytes.Count();
                if (startIndex > -1 && endIndex > -1 && length > 0)
                {
                    byte[] data = bytes.Skip(startIndex + startBytes.Count()).Take(length).ToArray();
                    bytesList.Add(data);
                    bytes = bytes.Skip(endIndex + endBytes.Count());
                }
                else
                {
                    break;
                }
            } while (true);
            return bytesList;
        }
        /// <summary>
        /// 根据头和长度切分bytes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_byte"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static List<byte[]> Capture(this IEnumerable<byte> bytes, IEnumerable<byte> startBytes, int dataLengthLocation, int dataLengthType)
        {
            List<byte[]> bytesList = new List<byte[]>();
            do
            {
                int startIndex = bytes.IndexOf(startBytes);
                if (startIndex > -1)
                {
                    byte[] _bytes = bytes.ToArray();
                    int length = dataLengthType switch
                    {
                        1 => _bytes[startIndex + dataLengthLocation],
                        2 => BitConverter.ToUInt16(_bytes, startIndex + dataLengthLocation),
                        4 => BitConverter.ToInt32(_bytes, startIndex + dataLengthLocation),
                        _ => throw new NotImplementedException()
                    };
                    byte[] data = bytes.Skip(startIndex + dataLengthLocation + dataLengthType).Take(length).ToArray();
                    bytesList.Add(data);
                    bytes = bytes.Skip(startIndex + dataLengthLocation + dataLengthType + length);
                }
                else
                {
                    break;
                }
            } while (true);
            return bytesList;
        }
        /// <summary>
        /// byte[]转string
        /// 注：默认使用UTF8编码
        /// </summary>
        /// <param name="bytes">byte[]数组</param>
        /// <returns></returns>
        public static string ToString(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// byte[]转string
        /// </summary>
        /// <param name="bytes">byte[]数组</param>
        /// <param name="encoding">指定编码</param>
        /// <returns></returns>
        public static string ToString(this byte[] bytes, Encoding encoding)
        {
            return encoding.GetString(bytes);
        }
        /// <summary>
        /// 将byte[]转为Base64字符串
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns></returns>
        public static string ToBase64String(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }
        /// <summary>
        /// 转为二进制字符串
        /// </summary>
        /// <param name="aByte">字节</param>
        /// <returns></returns>
        public static string ToBinString(this byte aByte)
        {
            return new byte[] { aByte }.ToBinString();
        }
        /// <summary>
        /// 转为二进制字符串
        /// 注:一个字节转为8位二进制
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns></returns>
        public static string ToBinString(this byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var aByte in bytes)
            {
                builder.Append(Convert.ToString(aByte, 2).PadLeft(8, '0'));
            }

            return builder.ToString();
        }
        /// <summary>
        /// Byte数组转为对应的16进制字符串
        /// </summary>
        /// <param name="bytes">Byte数组</param>
        /// <returns></returns>
        public static string To0XString(this byte[] bytes)
        {
            StringBuilder resStr = new StringBuilder();
            bytes.ToList().ForEach(aByte =>
            {
                resStr.Append(aByte.ToString("x2"));
            });

            return resStr.ToString();
        }
        /// <summary>
        /// Byte数组转为对应的16进制字符串
        /// </summary>
        /// <param name="aByte">一个Byte</param>
        /// <returns></returns>
        public static string To0XString(this byte aByte)
        {
            return new byte[] { aByte }.To0XString();
        }
        /// <summary>
        /// 转为ASCII字符串（一个字节对应一个字符）
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns></returns>
        public static string ToASCIIString(this byte[] bytes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bytes.ToList().ForEach(aByte =>
            {
                stringBuilder.Append((char)aByte);
            });
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 转为ASCII字符串（一个字节对应一个字符）
        /// </summary>
        /// <param name="aByte">字节数组</param>
        /// <returns></returns>
        public static string ToASCIIString(this byte aByte)
        {
            return new byte[] { aByte }.ToASCIIString();
        }
        /// <summary>
        /// 转为UTF-8字符串
        /// </summary>
        /// <param name="aByte">字节数组</param>
        /// <returns></returns>
        public static string ToUTF8String(this byte[] aByte)
        {
            return Encoding.UTF8.GetString(aByte);
        }
        /// <summary>
        /// 获取异或值
        /// 注：每个字节异或
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns></returns>
        public static byte GetXOR(this byte[] bytes)
        {
            int value = bytes[0];
            for (int i = 1; i < bytes.Length; i++)
            {
                value = value ^ bytes[i];
            }
            return (byte)value;
        }
        /// <summary>
        /// 将字节数组转为Int类型
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns></returns>
        public static int ToInt(this byte[] bytes)
        {
            int num = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                num += bytes[i] * ((int)Math.Pow(256, bytes.Length - i - 1));
            }
            return num;
        }
        /// <summary>
        /// 数据顺序
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sequenceType"></param>
        /// <returns></returns>
        public static byte[] DataSequence(this byte[] data, SortEnum sequenceType)
        {
            return sequenceType switch
            {
                SortEnum.BADC => data.HiloExchange(),
                SortEnum.DCBA => data.Reverse().ToArray(),
                SortEnum.CDAB => data.HiloExchange()?.Reverse().ToArray(),
                _ => data
            } ?? data;
        }
        /// <summary>
        /// 高低位互换
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[]? HiloExchange(this byte[]? data)
        {
            if (data == null) return null;
            if (data.Length > 1)
            {
                byte[] resultByt = new byte[data.Length - data.Length % 2];
                for (int i = 0; i < resultByt.Length; i += 2)
                {
                    resultByt[i] = data[i + 1];
                    resultByt[i + 1] = data[i];
                }
                return resultByt.ToArray();
            }
            else
            {
                return data;
            }
        }
    }
}
