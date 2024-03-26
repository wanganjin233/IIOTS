using IIOTS.Enum;
using IIOTS.Models;
using IIOTS.Util;
using System.Text.RegularExpressions;

namespace IIOTS.Driver
{
    public class S7 : BaseDriver
    {
        /// <summary>
        /// PLC的机架号
        /// </summary>
        private byte Rack
        {
            get
            {
                return rack;
            }
            set
            {
                rack = value;
                S7Command.FirstHandshake[21] = (byte)(rack * 32 + slot);
            }
        }
        private byte rack = 0;
        /// <summary>
        /// PLC的槽号
        /// </summary>
        private byte Slot
        {
            get
            {
                return slot;
            }
            set
            {
                slot = value;
                S7Command.FirstHandshake[21] = (byte)(rack * 32 + slot);
            }
        }
        private byte slot = 0;
        /// <summary>
        /// 驱动状态
        /// </summary> 
        public override bool DriverState => base.DriverState;
        /// <summary>
        /// 初始化西门子S7驱动
        /// </summary>
        /// <param name="ServerIP"></param>
        /// <param name="serverPort"></param>
        /// <param name="isPersistentConn"></param>
        public S7(string communicationStr)
            : base(communicationStr)
        {
            Communication.HeadBytes = [0x03, 0x00];
            Communication.DataLengthLocation = 2;
            Communication.DataLengthType = LengthTypeEnum.ReUint;
            _DriverType = DriverTypeEnum.S7;
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
            Tag lastTag = tagGroup.Tags.Last();
            if (tagGroup.IsBit)
            {
                tagGroup.Length = (ushort)((lastTag.Location - firstTag.Location) * 16 + lastTag.BitLocation - firstTag.BitLocation + 1);
            }
            return null;
        }
        public override void Start(int cycle = 100)
        {
            ThreadPool.QueueUserWorkItem(p =>
             {
                 if (IsRun == false)
                 {
                     while (true)
                     {
                         if (LogIn())
                         {
                             base.Start(cycle);
                             break;
                         }
                         Task.Delay(500);
                     }
                 }
             }, TaskCreationOptions.LongRunning);
        }
        /// <summary>
        /// S7协议两次握手
        /// </summary>
        /// <returns></returns>
        private bool LogIn()
        {
            return Communication != null && Communication.Send(S7Command.FirstHandshake)
                    && Communication.Receive().VerifyFirst()
                    && Communication.Send(S7Command.SecondHandshake)
                    && Communication.Receive().VerifySecond();
        }
        /// <summary>
        /// 读取最大长度
        /// </summary>
        public override int ReadMaxLength => 254;

        protected override List<TagGroup>? Packet(List<TagProcess> tags)
        {
            //生成组地址报文
            S7Addresses CreationS7Addresses(TagGroup tagGroup, AddressTypeEnum addressTypeEnum, ushort dbBlock)
            {
                Tag firstTag = tagGroup.Tags.First();
                Tag lastTag = tagGroup.Tags.Last();
                return new S7Addresses()
                {
                    Address = (ushort)firstTag.Location,
                    AddressType = addressTypeEnum,
                    Length = (ushort)(lastTag.Location + lastTag.DataLength - firstTag.Location),
                    DbBlock = dbBlock

                };

            }
            //清空分组
            TagGroups.Clear();
            if (tags.Count == 0) { return null; }
            var TagGroupKV = new Dictionary<TagGroup, List<S7Addresses>>();
            foreach (var tagGByTypeNeume in tags.GroupBy(p => p.Type))
            {
                var tagGroup = new TagGroup();
                TagGroupKV.Add(tagGroup, new List<S7Addresses>());
                foreach (var tagGByDbBlock in tagGByTypeNeume.GroupBy(p => p.DbBlock))
                {
                    //排序
                    List<TagProcess> tagsList = tagGByTypeNeume.OrderBy(p => p.Location).ToList();
                    int endTag = (int)(tagsList.First().Location + ReadMaxLength);
                    foreach (var tag in tagsList)
                    {
                        if (tag.Location + tag.DataLength < endTag)
                        {
                            tagGroup.Tags.Add(tag);
                        }
                        else //超出读取最大长度
                        {
                            TagGroupKV[tagGroup].Add(CreationS7Addresses(tagGroup, (AddressTypeEnum)tagGByTypeNeume.Key, tagGByDbBlock.Key)); 
                            tagGroup = new TagGroup();
                            tagGroup.Tags.Add(tag);
                            endTag = (int)(tag.Location + ReadMaxLength);
                        }
                    } 
                    TagGroupKV[tagGroup].Add(CreationS7Addresses(tagGroup, (AddressTypeEnum)tagGByTypeNeume.Key, tagGByDbBlock.Key));
                    if (TagGroupKV[tagGroup].Count > 10)
                    {
                        tagGroup = new TagGroup();
                    }
                }
                TagGroups.Add(tagGroup);
            }
            return TagGroups.ToList();
        }


        /// <summary>
        /// tag点位地址解析
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        protected override TagProcess TagParsing(TagProcess tag)
        {
            string? addressType = Regex.Matches(tag.Address, "^[a-zA-Z]+").FirstOrDefault()?.Value;
            if (addressType != null && addressType.ToEnum(out AddressTypeEnum _AddressType))
            {
                tag.Type = _AddressType;
                string[] addressArr = tag.Address[addressType.Length..].Split('.');
                if (tag.DataType == TagTypeEnum.Boole)
                {
                    if (addressArr.Length > 2)
                    {
                        tag.BitLocation = addressArr[2].ToInt();
                    }
                    else
                    {
                        tag.BitLocation = 0;
                    }
                    tag.IsBit = true;
                }
                tag.DbBlock = addressArr[0].ToUshort();
                tag.Location = (uint)addressArr[1].ToInt();
            }
            return tag;
        }
        /// <summary>
        /// 发送并接收
        /// </summary>
        /// <param name="command"></param>
        /// <param name="headByte"></param>
        /// <returns></returns>
        public override byte[]? SendCommand(byte[] command)
        {
            var bytes = base.SendCommand(command).GetBody();
            if (bytes == null)
            {
                while (true)
                {
                    if (LogIn())
                    {
                        break;
                    }
                    Task.Delay(500);
                }
            }
            return bytes;
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
            if (tag != null && value != null &&
                (tag.ClientAccess == ClientAccessEnum.RW
                || tag.ClientAccess == ClientAccessEnum.OW))
            {
                // byte[] command = ((ushort)tag.Location).BatchWriteCommand((AddressTypeEnum)tag.Type,
                //     tag.TagOnComm(value),
                //     tag.IsBit,
                //     PLCNode,
                //     PCNode,
                //     tag.StationNumber,
                //     (byte)tag.BitLocation);
                return SendCommand(new byte[0]) != null;
            }
            return false;
        }
    }
}
