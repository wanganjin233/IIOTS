using Opc.Ua.Client;
using Opc.Ua;
using IIOTS.Util;
using IIOTS.Models;
using IIOTS.Enums;

namespace IIOTS.Driver
{
    public class OPCUA : BaseDriver
    {
        private readonly OpcUaClient opcUaClient = new();
        private bool driverState = false;
        /// <summary>
        /// 连接状态
        /// </summary>
        public override bool DriverState => driverState;
        public override event Action<BaseDriver>? DriverStateChange;
        Dictionary<string, TagProcess> addressNames = [];
        /// <summary>
        /// 连接字符串
        /// </summary>
        private readonly string CommunicationStr;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="communicationStr"></param>
        /// <exception cref="Exception"></exception>
        public OPCUA(string communicationStr) : base()
        {
            _DriverType = DriverTypeEnum.OPCUA;
            if (communicationStr.Contains(';'))
            {
                string[] communicationStrs = communicationStr.Split(';');
                string[] user = communicationStrs[1].Split(':');
                opcUaClient.UserIdentity = new UserIdentity(user[0], user[1]);
                communicationStr = communicationStrs[0];
            }
            CommunicationStr = communicationStr;
        }
        /// <summary>
        /// opc状态变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpcUaClient_OpcStatusChange(object? sender, OpcUaStatusEventArgs e)
        {
            if (driverState != e.Connected)
            {
                driverState = e.Connected;
                if (!driverState)
                {
                    AllTags.ForEach(t =>
                    {
                        t.SetValue = null;
                    });
                }
                DriverStateChange?.Invoke(this);
            }
            foreach (var tag in AllTags)
            {
                tag.Timestamp = DateTime.Now;
            }
        }

        /// <summary>
        /// 启动驱动
        /// </summary>
        /// <param name="cycle"></param>
        public override void Start(int cycle = 100)
        {
            Task.Factory.StartNew(async () =>
            {
                if (IsRun == false)
                {
                    IsRun = true;
                    try
                    {
                        while (IsRun)
                        {
                            try
                            {
                                if (await opcUaClient.ConnectServer(CommunicationStr))
                                {
                                    opcUaClient.OpcStatusChange += OpcUaClient_OpcStatusChange;
                                    break;
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        addressNames = AllTags.ToDictionary(p => p.TagName, p => p);
                        //获取需要轮询的点位
                        var pollTagGroups = TagGroups
                              .Where(p => p.UpdateMode == UpdateModeEnum.Poll)
                              .ToList();
                        List<Tag> pollTags = [];
                        //合并组
                        foreach (var pollTagGroup in pollTagGroups)
                        {
                            pollTags = pollTags.Union(pollTagGroup.Tags).ToList();
                        }
                        //生成NodeId
                        NodeId[] tagNodeId = pollTags.Select(p => new NodeId(p.Address)).ToArray();
                        //获取全部订阅组
                        var subTagGroups = TagGroups
                                .Where(p => p.UpdateMode == UpdateModeEnum.Sub)
                                .ToList();
                        List<string> subTags = [];
                        //合并组
                        foreach (var subTagGroup in subTagGroups)
                        {
                            subTags = subTags.Union(subTagGroup.Tags.Select(p => p.Address)).ToList();
                        }
                        opcUaClient.RemoveAllSubscription();
                        //订阅点位
                        opcUaClient.AddSubscription(Guid.NewGuid().ToString("N"), subTags.ToArray(), SubCallback);
                        while (IsRun && tagNodeId.Length > 0)
                        {
                            try
                            {
                                //读取点位
                                List<DataValue> nodeDataValues = opcUaClient.ReadNodes(tagNodeId);
                                for (int i = 0; i < tagNodeId.Length; i++)
                                {
                                    TagProcess tagProcess = (TagProcess)pollTags[i];
                                    tagProcess.SetValue = nodeDataValues[i].Value;
                                }
                            }
                            catch (Exception)
                            {
                            }
                            Task.Delay(cycle).Wait();
                        }
                    }
                    catch (Exception e)
                    {
                        Stop();
                        throw new Exception($"OPC UA启动失败 {e.Message}");
                    }
                }
            }, TaskCreationOptions.LongRunning);

        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Write(string tagName, object? value)
        {
            if (value == null) return false;
            Tag? tag = AllTags.FirstOrDefault(p => p.TagName == tagName);
            if (tag == null) return false;
            return opcUaClient.WriteNode(tag.Address, tag.DataType switch
            {
                TagTypeEnum.Boole => Convert.ToBoolean(value.ToString()),
                TagTypeEnum.Ushort => Convert.ToUInt16(value.ToString()),
                TagTypeEnum.Short => Convert.ToInt16(value.ToString()),
                TagTypeEnum.Uint => Convert.ToUInt32(value.ToString()),
                TagTypeEnum.Int => Convert.ToInt32(value.ToString()),
                TagTypeEnum.Ulong => Convert.ToUInt64(value.ToString()),
                TagTypeEnum.Long => Convert.ToInt64(value.ToString()),
                TagTypeEnum.Float => Convert.ToSingle(value.ToString()),
                TagTypeEnum.Double => Convert.ToDouble(value.ToString()),
                TagTypeEnum.String => value.ToString() ?? string.Empty,
                TagTypeEnum.StringArray => value is string ? (value?.ToString()?.SplitToArray()) : value,
                _ => throw new NotImplementedException("无法找到合适的转换")
            });
        }
        /// <summary>
        /// 订阅回调
        /// </summary>
        /// <param name="key"></param>
        /// <param name="monitoredItem"></param>
        /// <param name="args"></param>
        private void SubCallback(string key, MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs args)
        {
            MonitoredItemNotification? notification = args.NotificationValue as MonitoredItemNotification;
            foreach (var tagProcess in addressNames.Values.Where(p => p.Address == monitoredItem.DisplayName))
            {
                if (notification?.Value.WrappedValue.TypeInfo == TypeInfo.Scalars.Boolean)
                {
                    tagProcess.DataType = TagTypeEnum.Boole;
                }
                else if (notification?.Value.WrappedValue.TypeInfo == TypeInfo.Scalars.Double)
                { 
                    tagProcess.DataType = TagTypeEnum.Double;
                }
                else if (notification?.Value.WrappedValue.TypeInfo == TypeInfo.Scalars.Float)
                { 
                    tagProcess.DataType = TagTypeEnum.Float;
                }
                else if (notification?.Value.WrappedValue.TypeInfo == TypeInfo.Scalars.String)
                {
                    tagProcess.DataType = TagTypeEnum.String;
                }
                else if (notification?.Value.WrappedValue.TypeInfo == TypeInfo.Scalars.Int16)
                {
                    tagProcess.DataType = TagTypeEnum.Short;
                }
                else if (notification?.Value.WrappedValue.TypeInfo == TypeInfo.Scalars.UInt16)
                {
                    tagProcess.DataType = TagTypeEnum.Ushort;
                }
                else if (notification?.Value.WrappedValue.TypeInfo == TypeInfo.Scalars.Int32)
                {
                    tagProcess.DataType = TagTypeEnum.Int;
                }
                else if (notification?.Value.WrappedValue.TypeInfo == TypeInfo.Scalars.UInt32)
                {
                    tagProcess.DataType = TagTypeEnum.Uint;
                }
                else if (notification?.Value.WrappedValue.TypeInfo == TypeInfo.Scalars.Int64)
                {
                    tagProcess.DataType = TagTypeEnum.Long;
                }
                else if (notification?.Value.WrappedValue.TypeInfo == TypeInfo.Scalars.UInt64)
                {
                    tagProcess.DataType = TagTypeEnum.Ulong;
                }
                tagProcess.SetValue = notification?.Value.WrappedValue.Value;
            }
        }
        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        protected override List<TagGroup>? Packet(List<TagProcess> tags)
        {
            TagGroups.Clear();
            var _tagGroups = tags.GroupBy(p => p.UpdateMode).ToList();
            foreach (var _tagGroup in _tagGroups)
            {
                TagGroup tagGroup = new()
                {
                    UpdateMode = _tagGroup.Key
                };
                tagGroup.Tags.AddRange(_tagGroup.ToList());
                TagGroups.Add(tagGroup);
            }
            return TagGroups.ToList();
        }
        /// <summary>
        /// 停止驱动
        /// </summary>
        public override void Stop()
        {
            opcUaClient.RemoveAllSubscription();
            opcUaClient.Disconnect();
            base.Stop();
        }
    }
}