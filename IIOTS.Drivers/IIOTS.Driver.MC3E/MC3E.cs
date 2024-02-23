using IIOTS.Enum;
using IIOTS.Models;
using IIOTS.Util;

namespace IIOTS.Driver
{
    public class MC3E : BaseDriver
    {
        public MC3E(string communicationStr)
           : base(communicationStr)
        {
            Communication.DataLengthLocation = 7;
            Communication.DataLengthType = LengthTypeEnum.UShort;
            _DriverType = DriverTypeEnum.MC3E;
        }
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
            return firstTag.Location.BatchReadCommand((AddressTypeEnum)TypeEnumtem, tagGroup.Length, firstTag.IsBit, NetworkNumber, StationNumber);
        }
        /// <summary>
        /// 网络号，通常为0
        /// </summary>
        /// <remarks>
        /// 依据PLC的配置而配置，如果PLC配置了1，那么此处也填0，如果PLC配置了2，此处就填2，测试不通的话，继续测试0
        /// </remarks>
        public byte NetworkNumber
        {
            get;
            set;
        } = 0;
        /// <summary>
        /// 读取最大长度
        /// </summary>
        public override int ReadMaxLength => 960;
        /// <summary>
        /// 发送并接收
        /// </summary>
        /// <param name="command"></param>
        /// <param name="headByte"></param>
        /// <returns></returns>
        public override byte[]? SendCommand(byte[] command)
        {
            int datalen = BitConverter.ToUInt16(new byte[2] { command[19], command[20] }, 0);
            Communication.HeadBytes = new byte[] {
                0xD0,
                command[1],
                command[2],
                command[3],
                command[4],
                command[5],
                command[6],
            };
            return base.SendCommand(command).GetBody(command[13] == 1, datalen);
        }

        /// <summary>
        /// tag点位地址解析
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
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
                tag.ClientAccess == ClientAccessEnum.RW)
                 )
            {
                byte[] command = tag.Location.BatchWriteCommand(
                    (AddressTypeEnum)tag.Type,
                    tag.TagOnComm(value),
                    tag.IsBit,
                    NetworkNumber,
                    tag.StationNumber);
                return SendCommand(command) != null;
            }
            return false;
        }


    }
}
