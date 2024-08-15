using IIOTS.Enums;
using IIOTS.Models;
using IIOTS.Util;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace IIOTS.Driver
{
    public partial class S7 : BaseDriver
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
            Communication.DataLengthType = LengthTypeEnum.ReUShort;
            Communication.LengthReplenish = -4;
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
                    IsRun = true;
                    while (IsRun)
                    {
                        if (LogIn())
                        { 
                            Task.Factory.StartNew(async () =>
                            {
                                bool state = true;
                                while (IsRun)
                                {
                                    foreach (var tagGroup in TagGroups)
                                    {
                                        try
                                        {
                                            if (tagGroup.Command == null)
                                            {
                                                continue;
                                            }
                                            byte[]? BodyByte = SendCommand(tagGroup.Command); 
                                            if (BodyByte != null)
                                            {
                                                foreach (var s7Addresses in TagGroupKVs[tagGroup])
                                                {
                                                    if (BodyByte.Equalsbytes([0xFF, 0x04]))
                                                    {
                                                        int length = BitConverter.ToUInt16(BodyByte
                                                            .Skip(2)
                                                            .Take(2)
                                                            .Reverse()
                                                            .ToArray()) / 8;
                                                        foreach (var tag in s7Addresses.Tags)
                                                        {
                                                            int skipIndex = (int)(tag.Location - s7Addresses.Address) + 4;
                                                            if (tag.IsBit)
                                                            {
                                                                tag.UpdateValue = [(byte)((BodyByte
                                                             .Skip(skipIndex)
                                                             .Take(1)
                                                             .First() >> tag.BitLocation)&1) ];
                                                            }
                                                            else
                                                            {
                                                                tag.UpdateValue = BodyByte
                                                                                .Skip(skipIndex)
                                                                                .Take(tag.DataLength)
                                                                                .ToArray();
                                                            }
                                                        }
                                                        BodyByte = BodyByte.Skip(length + 4).ToArray();
                                                    }
                                                    else
                                                    {
                                                        BodyByte = BodyByte.Skip(4).ToArray();
                                                    }
                                                }
                                                state = true;
                                            }
                                            else
                                            {
                                                state = false;
                                                break;
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            state = false;
                                            break;
                                        }
                                    } 
                                    if (state != State)
                                    {
                                        State = state;
                                        if (!State)
                                        {
                                            AllTags.ForEach(t =>
                                            {
                                                t.SetValue = null;
                                            });
                                        }
                                        ThreadPool.QueueUserWorkItem(p => DriverStateChange?.Invoke(this));
                                    }
                                    await Task.Delay(cycle);
                                }
                            }, TaskCreationOptions.LongRunning);
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
        private readonly ConcurrentDictionary<TagGroup, List<S7Addresses>> TagGroupKVs = new();
        //生成组地址报文
        private S7Addresses CreationS7Addresses(List<TagProcess> itemTags, AddressTypeEnum addressTypeEnum, ushort dbBlock)
        {
            Tag firstTag = itemTags.First();
            Tag lastTag = itemTags.Last();
            return new S7Addresses()
            {
                Address = (ushort)firstTag.Location,
                AddressType = addressTypeEnum,
                Length = (ushort)(lastTag.Location + lastTag.DataLength - firstTag.Location),
                DbBlock = dbBlock,
                Tags = itemTags
            };
        }
        /// <summary>
        /// tag分组
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        protected override List<TagGroup>? Packet(List<TagProcess> tags)
        { 
            //清空分组
            TagGroups.Clear();
            if (tags.Count == 0) { return null; }
            //根据点位类型分类遍历
            foreach (var tagGByTypeNeume in tags.GroupBy(p => p.Type))
            {
                var tagGroup = new TagGroup();
                TagGroupKVs.TryAdd(tagGroup, []);
                //遍历DB块分类
                foreach (var tagGByDbBlock in tagGByTypeNeume.GroupBy(p => p.DbBlock))
                {
                    List<TagProcess> itemTags = [];
                    //排序
                    List<TagProcess> tagsList = [.. tagGByDbBlock.OrderBy(p => p.Location)];
                    //计算一组结束位置
                    int endTag = (int)(tagsList.First().Location + ReadMaxLength);
                    S7Addresses s7Addresses;
                    //遍历块的tag
                    foreach (var tag in tagsList)
                    {
                        //点位结束位置小于组最大结束位置添加到组
                        if (tagGroup.Length + tag.Location + tag.DataLength < endTag)
                        { 
                            itemTags.Add(tag);
                            tagGroup.Tags.Add(tag);
                        }
                        else //超出读取最大长度
                        {
                            s7Addresses = CreationS7Addresses(itemTags, (AddressTypeEnum)tagGByTypeNeume.Key, tagGByDbBlock.Key);
                            TagGroupKVs[tagGroup].Add(s7Addresses);
                            tagGroup.Length += s7Addresses.Length;
                            tagGroup = new TagGroup();
                            TagGroupKVs.TryAdd(tagGroup, []);
                            tagGroup.Tags.Add(tag);
                            itemTags = [tag];
                            endTag = (int)(tag.Location + ReadMaxLength);
                        }
                    }
                    s7Addresses = CreationS7Addresses(itemTags, (AddressTypeEnum)tagGByTypeNeume.Key, tagGByDbBlock.Key);
                    TagGroupKVs[tagGroup].Add(s7Addresses);
                    tagGroup.Length += s7Addresses.Length;
                    if (TagGroupKVs[tagGroup].Count >= 1)
                    {
                        tagGroup = new TagGroup();
                        TagGroupKVs.TryAdd(tagGroup, []);
                    }
                }
            }
            foreach (var TagGroupKV in TagGroupKVs)
            {
                TagGroupKV.Key.Command = TagGroupKV.Value.ToArray().BatchReadCommand();
                TagGroups.Add(TagGroupKV.Key);
            }
            return [.. TagGroups];
        }
        /// <summary>
        /// 状态变化
        /// </summary>
        public override event Action<BaseDriver>? DriverStateChange;
        /// <summary>
        /// tag点位地址解析
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        protected override TagProcess TagParsing(TagProcess tag)
        {
            string? addressType = TagAddressMatches().Matches(tag.Address).FirstOrDefault()?.Value;
            if (addressType != null && addressType.ToEnum(out AddressTypeEnum _AddressType))
            {
                tag.Type = _AddressType;
                string[] addressArr = tag.Address[addressType.Length..].Split('.');
                if (_AddressType == AddressTypeEnum.M)
                {
                    if (tag.DataType == TagTypeEnum.Boole)
                    {
                        tag.BitLocation = addressArr.Length > 1 ? addressArr[1].ToInt() : 0;
                        tag.IsBit = true;
                    }
                    tag.DbBlock = 0;
                    tag.Location = addressArr[0].ToUshort();
                }
                else
                {
                    if (tag.DataType == TagTypeEnum.Boole)
                    {
                        tag.BitLocation = addressArr.Length > 2 ? addressArr[2].ToInt() : 0;
                        tag.IsBit = true;
                    }
                    tag.DbBlock = addressArr[0].ToUshort();
                    tag.Location = (uint)addressArr[1].ToInt();
                }
            }
            else
            {
                throw new Exception("地址类型错误");
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
                byte[] valueByte = tag.TagOnComm(value);
                byte[] command = new S7Addresses[1]{  new()
                {
                    BitLocation=tag.BitLocation,
                    IsBit=tag.IsBit,
                    Address = (ushort)tag.Location,
                    AddressType = (AddressTypeEnum)tag.Type,
                    Length = (ushort)valueByte.Length,
                    DbBlock = tag.DbBlock,
                    WriteData = valueByte
                }}.BatchWriteCommand();
                return SendCommand(command) != null;
            }
            return false;
        }

        [GeneratedRegex("^[a-zA-Z]+")]
        private static partial Regex TagAddressMatches();
    }
}
