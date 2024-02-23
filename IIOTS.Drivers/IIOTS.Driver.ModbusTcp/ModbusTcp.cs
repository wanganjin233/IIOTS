using IIOTS.Enum;
using IIOTS.Models;
using IIOTS.Util;

namespace IIOTS.Driver
{
    public class ModbusTcp : BaseDriver
    {
        #region 初始化  
        public ModbusTcp(string communicationStr)
           : base(communicationStr)
        {
            Communication.DataLengthLocation = 4;
            Communication.DataLengthType = LengthTypeEnum.ReUShort;
            _DriverType = DriverTypeEnum.ModbusTcp;
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
            return ((ushort)firstTag.Location).BatchReadCommand(tagGroup.Length, (byte)(AddressTypeEnum)TypeEnumtem, StationNumber);
        }
        /// <summary>
        /// tag点位地址解析
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected override TagProcess TagParsing(TagProcess tag)
        {
            switch (tag.Address[0])
            {
                case '0':
                    tag.Type = AddressTypeEnum.zero;
                    tag.IsBit = true;
                    break;
                case '1':
                    tag.Type = AddressTypeEnum.one;
                    tag.ClientAccess = ClientAccessEnum.OR;
                    tag.IsBit = true;
                    break;
                case '3':
                    tag.Type = AddressTypeEnum.threea;
                    tag.ClientAccess = ClientAccessEnum.OR;
                    break;
                case '4':
                    tag.Type = AddressTypeEnum.four;
                    break;
                default:
                    throw new Exception("地址错误");
            }
            if (tag.Address.Contains('.'))
            {
                tag.DataType = TagTypeEnum.Boole;
                var addressSplit = tag.Address.Split('.');
                addressSplit[1].ToInt();
                tag.Location = addressSplit[0][1..].ToUshort();
            }
            else
            {
                tag.Location = tag.Address[1..].ToUshort();
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
            if (tag!=null&& value != null&&(
                tag.ClientAccess == ClientAccessEnum.OW ||
                tag.ClientAccess == ClientAccessEnum.RW))
            {
                byte[] command = ((ushort)tag.Location).BatchWriteCommand(tag.TagOnComm(value),
                    tag.IsBit,
                    tag.StationNumber);
                return SendCommand(command) != null;
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
            Communication.HeadBytes = SetIdentifying(command);
            var ada = base.SendCommand(command).GetBody(command[7] == 2 || command[7] == 1, BitConverter.ToUInt16(command.Reverse().ToArray())); ;
            return ada;
        }
        #endregion
    }
}