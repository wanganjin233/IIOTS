using IIOTS.Enums;
using IIOTS.Models;
using IIOTS.Util;
using System.ComponentModel;

namespace IIOTS.Driver
{
    public class XINJIE : BaseDriver
    { 
        #region 初始化  
        public XINJIE(string communicationStr)
           : base(communicationStr)
        {
            Communication.DataLengthLocation = 4;
            Communication.DataLengthType = LengthTypeEnum.Byte;
            _DriverType = DriverTypeEnum.XINJIE;
        }

        #endregion
        #region 属性   
        /// <summary>
        /// 消息标识
        /// </summary>
        private ushort identifying = 0;
        #endregion
        #region 驱动私有方法
        /// <summary>
        /// 设置标识
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private byte[] SetIdentifying(byte[] command)
        {
            byte[] headByte = new byte[4];
            byte[] _Identifying = BitConverter.GetBytes(++identifying);
            _Identifying.CopyTo(headByte, 0);
            headByte.CopyTo(command, 0);
            return headByte;
        }
        #endregion
        #region 重写方法
        /// <summary>
        /// 生成报文
        /// </summary>
        /// <param name="tagGroup"></param>
        /// <param name="StationNumber"></param>
        /// <param name="TypeEnumtem"></param>
        /// <returns></returns>
        protected override byte[]? BatchReadCommand(TagGroup tagGroup, byte StationNumber, object TypeEnumtem)
        {
            Tag firstTag = tagGroup.Tags.First();
            return firstTag.Location.BatchReadCommand(tagGroup.Length, (AddressTypeEnum)TypeEnumtem, StationNumber);
        }
        /// <summary>
        /// tag点位地址解析
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected override TagProcess TagParsing(TagProcess tag)
        {
            string addressType = Tools.ABCTypeRegex().Matches(tag.Address).Single().Value;
            if (addressType.ToEnum(out AddressTypeEnum _AddressType))
            {
                tag.Type = _AddressType;
                tag.TagAddressTransform(_AddressType);
            }
            return tag;
        }
        /// <summary>
        ///  写入数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <param name="isBit"></param>
        /// <returns></returns> 
        public override bool Write(string tagName, object? value)
        {
            Tag? tag = AllTags.FirstOrDefault(p => p.TagName == tagName);
            if (tag != null && value != null && (
                tag.ClientAccess == ClientAccessEnum.OW ||
                tag.ClientAccess == ClientAccessEnum.RW))
            {
                var memberInfo = typeof(AddressTypeEnum)
                    .GetMember(((AddressTypeEnum)Enum.ToObject(typeof(AddressTypeEnum), tag.Type)).ToString());
                var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                var sign = ((DescriptionAttribute)attributes.Single()).Description;
                byte[] command = tag.Location.BatchWriteCommand(tag.TagOnComm(value),
                    (AddressTypeEnum)tag.Type,
                    sign == "Bit",
                    tag.StationNumber);
                byte[]? reData = base.SendCommand(command);
                if (reData != null && reData[7] == command[7] && reData[8] == command[8])
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override byte[]? SendCommand(byte[] command)
        {
            AddressTypeEnum addressType = (AddressTypeEnum)Enum.ToObject(typeof(AddressTypeEnum), BitConverter.ToUInt16([command[8], command[7]]));
            var memberInfo = typeof(AddressTypeEnum).GetMember(addressType.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            var sign = ((DescriptionAttribute)attributes.Single()).Description;
            Communication.HeadBytes = SetIdentifying(command);
            return base.SendCommand(command).GetBody(sign == "Bit", BitConverter.ToUInt16(command.Reverse().ToArray()));
        }
        #endregion
    }
}