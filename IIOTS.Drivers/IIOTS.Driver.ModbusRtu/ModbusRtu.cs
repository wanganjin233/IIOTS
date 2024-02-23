using IIOTS.Enum;
using IIOTS.Models;
using IIOTS.Util;

namespace IIOTS.Driver
{
    public class ModbusRtu : BaseDriver
    {
        public ModbusRtu(string ConnectionString)
           : base(ConnectionString)
        {
            Communication.DataLengthLocation = 2;
            Communication.LengthReplenish = 2;
            Communication.DataLengthType = LengthTypeEnum.Byte;
            _DriverType = DriverTypeEnum.ModbusRtu;
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
            return ((ushort)firstTag.Location).BatchReadCommand(tagGroup.Length, (byte)(AddressTypeEnum)TypeEnumtem, StationNumber);
        }
        /// <summary>
        /// 站号
        /// </summary>
        public byte StationNumber
        {
            get;
            set;
        } = 1;
        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override byte[]? SendCommand(byte[] command)
        {
            Communication.HeadBytes = command.Take(2).ToArray();
            bool isBit = command[1] == 2 || command[1] == 1;
            int readLenth = BitConverter.ToUInt16(command.Reverse().ToArray(), 2);
            byte[] headBytes = new byte[3];
            Communication.HeadBytes.CopyTo(headBytes, 0);
            headBytes[2] = isBit ? (byte)(command[2] / 8 + 1) : (byte)(readLenth * 2);
            var bytes = base.SendCommand(command);
            if (bytes != null)
            {
                return headBytes.AddBytes(bytes).GetBody(isBit, readLenth);
            }
            return null;
        }
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
            if (tag!=null&&(tag.ClientAccess == ClientAccessEnum.OW || tag.ClientAccess == ClientAccessEnum.RW)
                && value != null)
            {
                byte[] command = ((ushort)tag.Location).BatchWriteCommand(
                    tag.TagOnComm(value),
                    tag.IsBit,
                    tag.StationNumber);
                return SendCommand(command) != null;
            }
            return false;
        }
    }
}