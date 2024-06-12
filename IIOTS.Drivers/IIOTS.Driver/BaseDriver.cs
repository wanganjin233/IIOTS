using System.Collections.Concurrent;
using IIOTS.Communication;
using IIOTS.Enums;
using IIOTS.Interface;
using IIOTS.Models;
using IIOTS.Util;

namespace IIOTS.Driver
{
    public abstract class BaseDriver : IDisposable
    {
        public BaseDriver()
        {

        }
        public BaseDriver(string communicationStr)
        {
            Communication = Connect.ConnectionResolution(communicationStr);
            Communication.ReceiveTimeout = 5000;
            Communication.SendTimeout = 5000;
            Communication.Connect();
        }
        /// <summary>
        /// 驱动类型
        /// </summary>
        protected DriverTypeEnum _DriverType;
        /// <summary>
        /// 驱动类型
        /// </summary>
        public DriverTypeEnum DriverType => _DriverType;
        /// <summary>
        /// 锁
        /// </summary>
        private readonly object _lock = new();
        /// <summary>
        /// 状态变化
        /// </summary>
        public virtual event Action<BaseDriver>? DriverStateChange;
        /// <summary>
        /// 连接需求
        /// </summary>
        protected readonly ICommunication? Communication;
        /// <summary>
        ///最大 读取长度
        /// </summary>
        public virtual int ReadMaxLength => 124;
        protected bool State = true;
        /// <summary>
        /// 驱动连接状态
        /// </summary>
        public virtual bool DriverState => State;
        /// <summary>
        /// 点位组
        /// </summary>
        protected ConcurrentBag<TagGroup> TagGroups = [];
        /// <summary>
        /// 全部点位
        /// </summary>
        protected ConcurrentDictionary<string, TagProcess> AllTagDic = new();
        /// <summary>
        /// 全部点位
        /// </summary>
        public List<TagProcess> AllTags => AllTagDic.Values.ToList();
        /// <summary>
        /// 写入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception> 
        public virtual bool Write(string tagName, object? value)
        {
            throw new Exception("功能未实现");
        }

        /// <summary>
        /// 发送命令并返回
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual byte[]? SendCommand(byte[] command)
        {
            lock (_lock)
            {
                if (Communication?.Send(command) ?? false)
                {
                    Thread.Sleep(10);
                    return Communication?.Receive();
                }
                return null;
            }
        }
        /// <summary>
        /// 生成报文
        /// </summary>
        /// <param name="tagGroup"></param>
        /// <param name="StationNumber"></param>
        /// <param name="TypeEnumtem"></param>
        /// <returns></returns>
        protected virtual byte[]? BatchReadCommand(TagGroup tagGroup, byte StationNumber, object TypeEnumtem)
        {
            throw new Exception("未实现");
        }

        /// <summary>
        /// tag分组
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        protected virtual List<TagGroup>? Packet(List<TagProcess> tags)
        {
            //清空分组
            TagGroups.Clear();
            if (tags.Count == 0) { return null; }
            foreach (var tagGByStationNumber in tags.GroupBy(p => p.StationNumber))
            {
                foreach (var tagGByBit in tagGByStationNumber.GroupBy(p => p.IsBit))
                {
                    foreach (var tagGByTypeNeume in tagGByBit.GroupBy(p => p.Type))
                    {
                        TagGroup tagGroup = new()
                        {
                            IsBit = tagGByBit.Key
                        };
                        //排序
                        List<TagProcess> tagsList = tagGByTypeNeume.OrderBy(p => p.Location).ToList();
                        //生成组地址报文
                        void CreationReadCommand(TagGroup tagGroup)
                        {
                            Tag firstTag = tagGroup.Tags.First();
                            Tag lastTag = tagGroup.Tags.Last();
                            tagGroup.Length = (ushort)(lastTag.Location + Math.Ceiling(lastTag.DataLength / 2.0) - firstTag.Location);
                            tagGroup.Command = BatchReadCommand(tagGroup, tagGByStationNumber.Key, tagGByTypeNeume.Key);
                            tagGroup.StartAddress = (ushort)firstTag.Location;
                        }
                        //获取结束位置 
                        int GetEndPosition(Tag tag) => (int)(tag.Location + ReadMaxLength);
                        int endTag = GetEndPosition(tagsList.First());
                        foreach (var tag in tagsList)
                        {
                            if (tag.Location + tag.DataLength / 2 < endTag)
                            {
                                tagGroup.Tags.Add(tag);
                            }
                            else
                            {
                                TagGroups.Add(tagGroup);
                                CreationReadCommand(tagGroup);
                                tagGroup = new()
                                {
                                    IsBit = tagGByBit.Key
                                };
                                tagGroup.Tags.Add(tag);
                                endTag = GetEndPosition(tag);
                            }
                        }
                        TagGroups.Add(tagGroup);
                        CreationReadCommand(tagGroup);
                    }
                }
            }
            return TagGroups.ToList();
        }

        /// <summary>
        /// tag处理
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        protected virtual TagProcess TagParsing(TagProcess tag)
        {
            return tag;
        }
        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="tags"></param>
        public virtual void AddTags(List<Tag> tags)
        {
            tags.ToJson().ToObject<List<TagProcess>>()?.ForEach(p =>
            {
                p = TagParsing(p);
                AllTagDic.TryAdd(p.TagName, p);
                p.baseDriver = this;
            });
            Packet(AllTagDic.Values.ToList());
        }
        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="tags"></param>
        public void RemoveTags(List<string> tags)
        {
            tags.ForEach(p =>
            {
                AllTagDic.Remove(p, out TagProcess? tag);
            });
            Packet(AllTagDic.Values.ToList());
        }
        /// <summary>
        /// 启动驱动
        /// </summary>
        /// <param name="cycle"></param>
        public virtual void Start(int cycle = 100)
        {
            if (IsRun == false)
            {
                IsRun = true;
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
                                    tagGroup.Tags.ForEach(p =>
                                    {
                                        int skipIndex;
                                        if (tagGroup.IsBit)
                                        {
                                            if (p.BitLocation != -1)
                                            {
                                                skipIndex = (int)(p.Location - tagGroup.StartAddress) * 16 + p.BitLocation;
                                            }
                                            else
                                            {
                                                skipIndex = (int)(p.Location - tagGroup.StartAddress);
                                            }
                                        }
                                        else
                                        {
                                            skipIndex = (int)(p.Location - tagGroup.StartAddress) * 2;
                                        }
                                       ((TagProcess)p).UpdateValue = BodyByte
                                        .Skip(skipIndex)
                                        .Take(p.DataLength)
                                        .ToArray();
                                    });
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
                            }
                        }

                        if (state != State)
                        {
                            State = state;
                            ThreadPool.QueueUserWorkItem(p => DriverStateChange?.Invoke(this));
                        }
                        await Task.Delay(cycle);
                    }
                }, TaskCreationOptions.LongRunning);
            }
        }
        /// <summary>
        /// 运行状态
        /// </summary>
        protected bool IsRun = false; 

        /// <summary>
        /// 停止驱动
        /// </summary>
        public virtual void Stop()
        {
            Communication?.Dispose();
            IsRun = false;
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}
