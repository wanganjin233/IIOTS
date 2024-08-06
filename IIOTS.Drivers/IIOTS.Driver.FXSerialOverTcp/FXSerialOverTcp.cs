using IIOTS.Enums;
using IIOTS.Interface;
using IIOTS.Models;
using IIOTS.Util;
using System.Net;
using System.Text;

namespace IIOTS.Driver
{
    public class FXSerialOverTcp : BaseDriver
    {
        public FXSerialOverTcp(string communicationStr)
           : base(communicationStr)
        {
            Communication.HeadBytes = [0x02];
            Communication.EndBytes = [0x03];
            Communication.LengthReplenish = 2;
            _DriverType = DriverTypeEnum.FXSerialOverTcp;
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
            return ((ushort)firstTag.Location).BatchReadCommand((AddressTypeEnum)TypeEnumtem, (byte)tagGroup.Length, firstTag.IsBit);
        }
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
            int datalen = command.Skip(6).Take(2).ToArray().ToASCIIString().ToInt();
            return base.SendCommand(command).GetBody(datalen);
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
                byte[] command = ((ushort)tag.Location).BatchWriteCommand(
                    (AddressTypeEnum)tag.Type,
                    tag.TagOnComm(value),
                    tag.IsBit);
                return SendCommand(command)?.Equalsbytes([0x06]) ?? false;
            }
            return false;
        }


    }
}
