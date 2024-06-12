using IIOTS.Enums;
using IIOTS.Models;
using IIOTS.Util;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

namespace IIOTS.Driver
{
    public static partial class Tools
    {
        /// <summary>
        /// 字母开头类型的地址正则
        /// </summary>
        /// <returns></returns>
        [GeneratedRegex("^[a-zA-Z]+")]
        public static partial Regex ABCTypeRegex();
        /// <summary>
        /// 字母+数字开头类型的地址正则
        /// </summary>
        /// <returns></returns>
        [GeneratedRegex("^[E][A-Z0-9]")]
        public static partial Regex ABCNumberTypeRegex();
        /// <summary>
        /// 生成Tag写入命令
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static byte[] TagOnComm(this Tag tag, object value)
        {
            byte[] bytes = tag.DataType switch
            {
                TagTypeEnum.Boole => BitConverter.GetBytes(Convert.ToBoolean(value.ToString())),
                TagTypeEnum.Ushort => BitConverter.GetBytes(Convert.ToUInt16(value.ToString())),
                TagTypeEnum.Short => BitConverter.GetBytes(Convert.ToInt16(value.ToString())),
                TagTypeEnum.Uint => BitConverter.GetBytes(Convert.ToUInt32(value.ToString())),
                TagTypeEnum.Int => BitConverter.GetBytes(Convert.ToInt32(value.ToString())),
                TagTypeEnum.Ulong => BitConverter.GetBytes(Convert.ToUInt64(value.ToString())),
                TagTypeEnum.Long => BitConverter.GetBytes(Convert.ToInt64(value.ToString())),
                TagTypeEnum.Float => BitConverter.GetBytes(Convert.ToSingle(value.ToString())),
                TagTypeEnum.Double => BitConverter.GetBytes(Convert.ToDouble(value.ToString())),
                TagTypeEnum.String => Encoding.GetEncoding(tag.Coding.ToString()).GetBytes((value.ToString() ?? string.Empty).PadRight(tag.DataLength, '\0')),
                _ => throw new NotImplementedException("无法找到合适的转换")
            };
            return bytes.DataSequence(tag.Sort);
        }

        public static Tag TagAddressTransform<T>(this Tag tag, T AddressType) where T : notnull
        {
            var addressType = AddressType.ToString();
            if (addressType != null)
            {
                string address;
                if (tag.Address.Contains('.'))
                {
                    var addressSplit = tag.Address.Split('.');
                    address = addressSplit[0].Remove(0, addressType.Length);
                    tag.BitLocation = addressSplit[1].ToInt();
                }
                else
                {
                    address = tag.Address.Remove(0, addressType.Length);
                }
                var memberInfo = typeof(T).GetMember(addressType);
                var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                switch (((DescriptionAttribute)attributes.Single()).Description)
                {
                    case "BitHex":
                        tag.Location = (uint)address.ToInt0X();
                        tag.IsBit = true;
                        break;
                    case "Bit":
                        tag.Location = (uint)address.ToInt();
                        tag.IsBit = true;
                        break;
                    case "Word":
                        tag.Location = (uint)address.ToInt();
                        break;
                    case "WordHex":
                        tag.Location = (uint)address.ToInt0X();
                        break;
                };
            }
            return tag;
        }
    }
}
