using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IIOTS.Driver
{

    /// <summary>
    /// 一个二次封装了的OPC UA库，支持从opc ua服务器读写节点数据，批量读写，订阅，批量订阅，历史数据读取，方法调用操作。
    /// </summary>
    public class OpcUaClient : INotifyPropertyChanged
    {
        #region Constructors 
        /// <summary>
        /// 默认无参构造函数-无认证匿名方式登录
        /// </summary>
        public OpcUaClient()
        {
            #region 权限认证部分
            var certificateValidator = new CertificateValidator();//证书
            //证书验证事件
            certificateValidator.CertificateValidation += (sender, eventArgs) =>
            {
                if (ServiceResult.IsGood(eventArgs.Error))
                    eventArgs.Accept = true;
                else if (eventArgs.Error.StatusCode.Code == StatusCodes.BadCertificateUntrusted)
                    eventArgs.Accept = true;
                else
                    throw new Exception(
                        $"Failed to validate certificate with error code {eventArgs.Error.Code}: {eventArgs.Error.AdditionalInfo}");
            };
            //安全配置
            SecurityConfiguration securityConfigurationcv = new SecurityConfiguration
            {
                AutoAcceptUntrustedCertificates = true,
                RejectSHA1SignedCertificates = false,
                MinimumCertificateKeySize = 1024,
            };
            //证书验证部分更新安全配置
            certificateValidator.Update(securityConfigurationcv);
            #endregion

            // UA客户端配置=>标准
            application = new ApplicationInstance
            {
                ApplicationType = ApplicationType.Client,
                ConfigSectionName = OpcUaName,
                ApplicationConfiguration = new ApplicationConfiguration
                {
                    ApplicationName = OpcUaName,
                    ApplicationType = ApplicationType.Client,
                    CertificateValidator = certificateValidator,
                    ServerConfiguration = new ServerConfiguration
                    {
                        MaxSubscriptionCount = 100000,
                        MaxMessageQueueSize = 1000000,
                        MaxNotificationQueueSize = 1000000,
                        MaxPublishRequestCount = 10000000,
                    },

                    SecurityConfiguration = new SecurityConfiguration
                    {
                        AutoAcceptUntrustedCertificates = true,
                        RejectSHA1SignedCertificates = false,
                        MinimumCertificateKeySize = 1024,
                    },

                    TransportQuotas = new TransportQuotas
                    {
                        OperationTimeout = 6000000,
                        MaxStringLength = int.MaxValue,
                        MaxByteStringLength = int.MaxValue,
                        MaxArrayLength = 65535,
                        MaxMessageSize = 419430400,
                        MaxBufferSize = 65535,
                        ChannelLifetime = -1,
                        SecurityTokenLifetime = -1
                    },
                    ClientConfiguration = new ClientConfiguration
                    {
                        DefaultSessionTimeout = -1,
                        MinSubscriptionLifetime = -1,
                    },
                    DisableHiResClock = true
                }
            };
            m_configuration = application.ApplicationConfiguration;
        }
        #endregion

        #region Connect And Disconnect

        /// <summary>
        /// connect to server
        /// </summary>
        /// <param name="serverUrl">OPC服务器地址</param>
        /// <returns>连接是否成功</returns>
        public async Task<bool> ConnectServer(string serverUrl)
        {
            m_session = await Connect(serverUrl);
            return m_session != null;
        }
        /// <summary>
        /// Create  a new session
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <returns></returns>
        private async Task<Session> Connect(string serverUrl)
        {
            // disconnect from existing session.
            Disconnect();

            if (m_configuration == null)
            {
                throw new ArgumentNullException("m_configuration");
            }
            // select the best endpoint.
            EndpointDescription endpointDescription = CoreClientUtils.SelectEndpoint(serverUrl, UseSecurity);
            //Endpoint Cfg
            EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(m_configuration);
            ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);
            //创建会话
            m_session = await Session.Create(
                m_configuration,
                endpoint,
                false,
                false,
                (string.IsNullOrEmpty(OpcUaName)) ? m_configuration.ApplicationName : OpcUaName,
                60000,
                UserIdentity,
                Array.Empty<string>());

            // set up keep alive callback. 回调十分频繁 不要直接更新界面    最好包一层 利用观察者  状态改变再更新界面
            m_session.KeepAlive += Session_KeepAlive;

            // update the client status
            ConnectedState = true;

            // raise an event =>连接完成
            DoConnectComplete(null);

            // return the new session.
            return m_session;
        }

       
        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        public void Disconnect()
        {
            UpdateStatus(false, DateTime.UtcNow, "Disconnected");

            // stop any reconnect operation.
            if (m_reconnectHandler != null)
            {
                m_reconnectHandler.Dispose();
                m_reconnectHandler = null;
            }

            // disconnect any existing session.
            if (m_session != null)
            {
                m_session.Close(10000);
                m_session = null;
            }

            // update the client status
            ConnectedState = false;

            // raise an event.
            DoConnectComplete(null);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Report the client status
        /// </summary>
        /// <param name="connected">连接是否成功</param>
        /// <param name="time">The time associated with the status.</param>
        /// <param name="status">The status message.</param>
        /// <param name="args">Arguments used to format the status message.</param>
        private void UpdateStatus(bool connected, DateTime time, string status, params object[] args)
        {
            m_OpcStatusChange?.Invoke(this, new OpcUaStatusEventArgs()
            {
                Connected = connected,
                Time = time.ToLocalTime(),
                Text = string.Format(status, args),
            });
        }
       

        /// <summary>
        /// Handles a keep alive event from a session.
        /// </summary>
        private void Session_KeepAlive(ISession session, KeepAliveEventArgs e)
        {
            try
            {
                // 检验是否是同一个会话的回调
                if (!ReferenceEquals(session, m_session))
                {
                    return;
                } 
                // 判断是否是连接错误
                if (ServiceResult.IsBad(e.Status))
                {
                    if (m_reconnectPeriod <= 0)
                    {
                        UpdateStatus(false, e.CurrentTime, "Communication Error ({0})", e.Status);
                        return;
                    }

                    UpdateStatus(false, e.CurrentTime, "Reconnecting in {0}s", m_reconnectPeriod);

                    if (m_reconnectHandler == null)
                    {
                        m_ReconnectStarting?.Invoke(this, e);

                        m_reconnectHandler = new SessionReconnectHandler();
                        m_reconnectHandler.BeginReconnect(m_session, m_reconnectPeriod * 1000, Server_ReconnectComplete);
                    }

                    return;
                }

                // update status.
                UpdateStatus(true, e.CurrentTime, "Connected [{0}]", session.Endpoint.EndpointUrl);

                // raise any additional notifications.
                m_KeepAliveComplete?.Invoke(this, e);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Handles a reconnect event complete from the reconnect handler.
        /// </summary>
        private void Server_ReconnectComplete(object sender, EventArgs e)
        {
            try
            {
                // ignore callbacks from discarded objects.
                if (!Object.ReferenceEquals(sender, m_reconnectHandler))
                {
                    return;
                }

                m_session = (Session)m_reconnectHandler.Session;
                m_reconnectHandler.Dispose();
                m_reconnectHandler = null;

                // raise any additional notifications.
                m_ReconnectComplete?.Invoke(this, e);
            }
            catch (Exception)
            {

            }
        }

        #endregion

        #region LogOut Setting

        /// <summary>
        /// 设置OPC客户端的日志输出
        /// </summary>
        /// <param name="filePath">完整的文件路径</param>
        /// <param name="deleteExisting">是否删除原文件</param>
        public void SetLogPathName(string filePath, bool deleteExisting)
        {
            Utils.SetTraceLog(filePath, deleteExisting);
            Utils.SetTraceMask(515);
        }

        #endregion

        #region Public Members

        /// <summary>
        /// a name of application name show on server
        /// </summary>
        public string OpcUaName { get; set; } = "Opc Ua Helper";

        /// <summary>
        /// Whether to use security when connecting.
        /// </summary>
        public bool UseSecurity
        {
            get { return m_useSecurity; }
            set { m_useSecurity = value; }
        }

        /// <summary>
        /// 创建用户时要使用的用户标识
        /// </summary>
        public IUserIdentity UserIdentity { get; set; }

        /// <summary>
        /// 通信核心会话
        /// </summary>
        public Session? Session
        {
            get { return m_session; }
        }

        /// <summary>
        ///重连期限
        /// </summary>
        public int ReconnectPeriod
        {
            get => m_reconnectPeriod;
            set => m_reconnectPeriod = value;
        }

        /// <summary>
        /// Raised when a good keep alive from the server arrives.
        /// </summary>
        public event EventHandler KeepAliveComplete
        {
            add { m_KeepAliveComplete += value; }
            remove { m_KeepAliveComplete -= value; }
        }

        /// <summary>
        /// 重连开始
        /// </summary>
        public event EventHandler ReconnectStarting
        {
            add { m_ReconnectStarting += value; }
            remove { m_ReconnectStarting -= value; }
        }


        /// <summary>
        /// 重连完成
        /// </summary>
        public event EventHandler ReconnectComplete
        {
            add { m_ReconnectComplete += value; }
            remove { m_ReconnectComplete -= value; }
        }

        /// <summary>
        /// 连接完成
        /// </summary>
        public event EventHandler ConnectComplete
        {
            add { m_ConnectComplete += value; }
            remove { m_ConnectComplete -= value; }
        }
        /// <summary>
        /// Opc状态变化
        /// </summary>
        public event EventHandler<OpcUaStatusEventArgs> OpcStatusChange
        {
            add { m_OpcStatusChange += value; }
            remove { m_OpcStatusChange -= value; }
        }
        /// <summary>
        /// 配置信息
        /// </summary>
        public ApplicationConfiguration AppConfig
        {
            get
            {
                return m_configuration;
            }
        }

        #endregion

        #region Node Write/Read Support

        /// <summary>
        /// 读取不确定类型数据 返回datavalue
        /// </summary>
        /// <param name="nodeId">node id</param>
        /// <returns>DataValue</returns>
        public DataValue ReadNode(NodeId nodeId)
        {
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection
            {
                new ReadValueId( )
                {
                    NodeId = nodeId,
                    AttributeId = Attributes.Value
                }
            };

            // read the current value
            m_session.Read(
                null,
                0,
                TimestampsToReturn.Neither,
                nodesToRead,
                out DataValueCollection results,
                out DiagnosticInfoCollection diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

            return results[0];
        }

        /// <summary>
        /// 读取单个节点
        /// </summary>
        /// <typeparam name="T">type of value</typeparam>
        /// <param name="tag">node id</param>
        /// <returns>实际值</returns>
        public T ReadNode<T>(string tag)
        {
            DataValue dataValue = ReadNode(new NodeId(tag));
            return (T)dataValue.Value;
        }


        /// <summary>
        ///异步读取单个节点
        /// </summary>
        /// <typeparam name="T">The type of tag to read</typeparam>
        /// <param name="tag">tag值</param>
        /// <returns>The value retrieved from the OPC</returns>
        public Task<T> ReadNodeAsync<T>(string tag)
        {
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection
            {
                new ReadValueId()
                {
                    NodeId = new NodeId(tag),
                    AttributeId = Attributes.Value
                }
            };

            // Wrap the ReadAsync logic in a TaskCompletionSource, so we can use C# async/await syntax to call it:
            var taskCompletionSource = new TaskCompletionSource<T>();
            m_session.BeginRead(
                requestHeader: null,
                maxAge: 0,
                timestampsToReturn: TimestampsToReturn.Neither,
                nodesToRead: nodesToRead,
                callback: ar =>
                {
                    var response = m_session.EndRead(
                      result: ar,
                      results: out DataValueCollection results,
                      diagnosticInfos: out DiagnosticInfoCollection diag);

                    try
                    {
                        CheckReturnValue(response.ServiceResult);
                        CheckReturnValue(results[0].StatusCode);
                        var val = results[0];
                        taskCompletionSource.TrySetResult((T)val.Value);
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.TrySetException(ex);
                    }
                },
                asyncState: null);

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 批量读取节点
        /// </summary>
        /// <param name="nodeIds">all NodeIds</param>
        /// <returns>all values</returns>
        public List<DataValue> ReadNodes(NodeId[] nodeIds)
        {
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
            for (int i = 0; i < nodeIds.Length; i++)
            {
                nodesToRead.Add(new ReadValueId()
                {
                    NodeId = nodeIds[i],
                    AttributeId = Attributes.Value
                });
            }

            // 读取当前的值
            m_session.Read(
                null,
                0,
                TimestampsToReturn.Neither,
                nodesToRead,
                out DataValueCollection results,
                out DiagnosticInfoCollection diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

            return results.ToList();
        }

        /// <summary>
        /// 批量异步读取
        /// </summary>
        /// <param name="nodeIds">all NodeIds</param>
        /// <returns>all values</returns>
        public Task<List<DataValue>> ReadNodesAsync(NodeId[] nodeIds)
        {
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
            for (int i = 0; i < nodeIds.Length; i++)
            {
                nodesToRead.Add(new ReadValueId()
                {
                    NodeId = nodeIds[i],
                    AttributeId = Attributes.Value
                });
            }

            var taskCompletionSource = new TaskCompletionSource<List<DataValue>>();
            // 读取当前的值
            m_session.BeginRead(
                null,
                0,
                TimestampsToReturn.Neither,
                nodesToRead,
                callback: ar =>
                {
                    var response = m_session.EndRead(
                      result: ar,
                      results: out DataValueCollection results,
                      diagnosticInfos: out DiagnosticInfoCollection diag);

                    try
                    {
                        CheckReturnValue(response.ServiceResult);
                        taskCompletionSource.TrySetResult(results.ToList());
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.TrySetException(ex);
                    }
                },
                asyncState: null);

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 批量读取确定数据类型的节点
        /// </summary>
        /// <param name="tags">所以的节点数组信息</param>
        /// <returns>all values</returns>
        public List<T> ReadNodes<T>(string[] tags)
        {
            List<T> result = [];
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
            for (int i = 0; i < tags.Length; i++)
            {
                nodesToRead.Add(new ReadValueId()
                {
                    NodeId = new NodeId(tags[i]),
                    AttributeId = Attributes.Value
                });
            }

            // 读取当前的值
            m_session.Read(
                null,
                0,
                TimestampsToReturn.Neither,
                nodesToRead,
                out DataValueCollection results,
                out DiagnosticInfoCollection diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

            foreach (var item in results)
            {
                result.Add((T)item.Value);
            }
            return result;
        }

        /// <summary>
        /// 异步批量读取确定类型的节点
        /// </summary>
        /// <param name="tags">all NodeIds</param>
        /// <returns>all values</returns>
        public Task<List<T>> ReadNodesAsync<T>(string[] tags)
        {
            ReadValueIdCollection nodesToRead = [];
            for (int i = 0; i < tags.Length; i++)
            {
                nodesToRead.Add(new ReadValueId()
                {
                    NodeId = new NodeId(tags[i]),
                    AttributeId = Attributes.Value
                });
            }

            var taskCompletionSource = new TaskCompletionSource<List<T>>();
            // 读取当前的值
            m_session.BeginRead(
                null,
                0,
                TimestampsToReturn.Neither,
                nodesToRead,
                callback: ar =>
                {
                    var response = m_session.EndRead(
                      result: ar,
                      results: out DataValueCollection results,
                      diagnosticInfos: out DiagnosticInfoCollection diag);

                    try
                    {
                        CheckReturnValue(response.ServiceResult);
                        List<T> result = [];
                        foreach (var item in results)
                        {
                            result.Add((T)item.Value);
                        }
                        taskCompletionSource.TrySetResult(result);
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.TrySetException(ex);
                    }
                },
                asyncState: null);

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 写单个节点
        /// </summary>
        /// <typeparam name="T">The type of tag to write on</typeparam>
        /// <param name="tag">节点名称</param>
        /// <param name="value">值</param>
        /// <returns>写成功标志</returns>
        public bool WriteNode<T>(string tag, T value)
        {
            WriteValue valueToWrite = new WriteValue()
            {
                NodeId = new NodeId(tag),
                AttributeId = Attributes.Value
            };
            valueToWrite.Value.Value = value;
            valueToWrite.Value.StatusCode = StatusCodes.Good;
            valueToWrite.Value.ServerTimestamp = DateTime.MinValue;
            valueToWrite.Value.SourceTimestamp = DateTime.MinValue;

            WriteValueCollection valuesToWrite = new WriteValueCollection
            {
                valueToWrite
            };

            // 写入当前的值
            var ar = m_session.BeginWrite(
                requestHeader: null,
                nodesToWrite: valuesToWrite,
                callback: null,
                asyncState: null);

            if (!ar.AsyncWaitHandle.WaitOne(2000))
            {
                throw new Exception("超时时间已到，操作未完成");
            }

            m_session.EndWrite(
                ar,
                out StatusCodeCollection results,
                out DiagnosticInfoCollection diagnosticInfos);

            ClientBase.ValidateResponse(results, valuesToWrite);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, valuesToWrite);

            if (StatusCode.IsBad(results[0]))
            {
                //throw new ServiceResultException( results[0] );
                throw new Exception(string.Format("OPC UA Driver WriteNode Error: {0}", results[0]));
            }

            return !StatusCode.IsBad(results[0]); 
        }


        /// <summary>
        /// 异步单个写 
        /// </summary>
        /// <typeparam name="T">The type of tag to write on</typeparam>
        /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name. E.g: the tag `foo.bar` writes on the tag `bar` on the folder `foo`</param>
        /// <param name="value">The value for the item to write</param>
        public Task<bool> WriteNodeAsync<T>(string tag, T value)
        {
            WriteValue valueToWrite = new()
            {
                NodeId = new NodeId(tag),
                AttributeId = Attributes.Value,
            };
            valueToWrite.Value.Value = value;
            valueToWrite.Value.StatusCode = StatusCodes.Good;
            valueToWrite.Value.ServerTimestamp = DateTime.MinValue;
            valueToWrite.Value.SourceTimestamp = DateTime.MinValue;
            WriteValueCollection valuesToWrite =
            [
                valueToWrite
            ];

            // Wrap the WriteAsync logic in a TaskCompletionSource, so we can use C# async/await syntax to call it:
            var taskCompletionSource = new TaskCompletionSource<bool>();
            m_session.BeginWrite(
                requestHeader: null,
                nodesToWrite: valuesToWrite,
                callback: ar =>
                {
                    var response = m_session.EndWrite(
                      result: ar,
                      results: out StatusCodeCollection results,
                      diagnosticInfos: out DiagnosticInfoCollection diag);

                    try
                    {
                        ClientBase.ValidateResponse(results, valuesToWrite);
                        ClientBase.ValidateDiagnosticInfos(diag, valuesToWrite);
                        taskCompletionSource.SetResult(StatusCode.IsGood(results[0]));
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.TrySetException(ex);
                    }
                },
                asyncState: null);
            return taskCompletionSource.Task;
        }


        /// <summary>
        /// 所有的节点都写入成功，返回<c>True</c>，否则返回<c>False</c>
        /// </summary>
        /// <param name="tags">节点名称数组</param>
        /// <param name="values">节点的值数据</param>
        /// <returns>所有的是否都写入成功</returns>
        public bool WriteNodes(string[] tags, object[] values)
        {
            WriteValueCollection valuesToWrite = [];

            for (int i = 0; i < tags.Length; i++)
            {
                if (i < values.Length)
                {
                    WriteValue valueToWrite = new()
                    {
                        NodeId = new NodeId(tags[i]),
                        AttributeId = Attributes.Value
                    };
                    valueToWrite.Value.Value = values[i];
                    valueToWrite.Value.StatusCode = StatusCodes.Good;
                    valueToWrite.Value.ServerTimestamp = DateTime.MinValue;
                    valueToWrite.Value.SourceTimestamp = DateTime.MinValue;
                    valuesToWrite.Add(valueToWrite);
                }
            }

            // 写入当前的值

            m_session.Write(
                null,
                valuesToWrite,
                out StatusCodeCollection results,
                out DiagnosticInfoCollection diagnosticInfos);

            ClientBase.ValidateResponse(results, valuesToWrite);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, valuesToWrite);

            bool result = true;
            foreach (var r in results)
            {
                if (StatusCode.IsBad(r))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        #endregion
        #region Monitor Support


        /// <summary>
        /// 新增一个订阅，需要指定订阅的关键字，订阅的tag名，以及回调方法
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="tag">tag</param>
        /// <param name="callback">回调方法</param>
        public void AddSubscription(string key, string tag, Action<string, MonitoredItem, MonitoredItemNotificationEventArgs> callback)
        {
            AddSubscription(key, new string[] { tag }, callback);
        }

        /// <summary>
        /// 新增一批订阅，需要指定订阅的关键字，订阅的tag名数组，以及回调方法
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="tags">节点名称数组</param>
        /// <param name="callback">回调方法</param>
        public void AddSubscription(string key, string[] tags, Action<string, MonitoredItem, MonitoredItemNotificationEventArgs> callback)
        {
            Subscription m_subscription = new(m_session.DefaultSubscription)
            {
                PublishingEnabled = true,
                PublishingInterval = 0,
                KeepAliveCount = uint.MaxValue,
                LifetimeCount = uint.MaxValue,
                MaxNotificationsPerPublish = uint.MaxValue,
                Priority = 100,
                DisplayName = key
            };
            var count = m_session.SubscriptionCount;
            for (int i = 0; i < tags.Length; i++)
            {
                var item = new MonitoredItem
                {
                    StartNodeId = new NodeId(tags[i]),
                    AttributeId = Attributes.Value,
                    DisplayName = tags[i],
                    SamplingInterval = 100,
                };
                item.Notification += (MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs args) =>
                {
                    callback?.Invoke(key, monitoredItem, args);
                };
                m_subscription.AddItem(item);
            }


            m_session.AddSubscription(m_subscription);
            m_subscription.Create();

            lock (dic_subscriptions)
            {
                if (dic_subscriptions.TryGetValue(key, out Subscription? value))
                {
                    value.Delete(true);
                    m_session.RemoveSubscription(value);
                    value.Dispose();
                    dic_subscriptions[key] = m_subscription;
                }
                else
                {
                    dic_subscriptions.Add(key, m_subscription);
                }
            }
        }

        /// <summary>
        /// 移除订阅消息，如果该订阅消息是批量的，也直接移除
        /// </summary>
        /// <param name="key">订阅关键值</param>
        public void RemoveSubscription(string key)
        {
            lock (dic_subscriptions)
            {
                if (dic_subscriptions.TryGetValue(key, out Subscription? value))
                {
                    value.Delete(true);
                    m_session.RemoveSubscription(value);
                    value.Dispose();
                    dic_subscriptions.Remove(key);
                }
            }
        }


        /// <summary>
        /// 移除所有的订阅消息
        /// </summary>
        public void RemoveAllSubscription()
        {
            lock (dic_subscriptions)
            {
                foreach (var item in dic_subscriptions)
                {
                    item.Value.Delete(true);
                    m_session.RemoveSubscription(item.Value);
                    item.Value.Dispose();
                }
                dic_subscriptions.Clear();
            }
        }


        #endregion

        #region ReadHistory Support

        /// <summary>
        /// read History data
        /// </summary>
        /// <param name="tag">节点的索引</param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="count">读取的个数</param>
        /// <param name="containBound">是否包含边界</param>
        /// <returns>读取的数据列表</returns>
        public IEnumerable<DataValue> ReadHistoryRawDataValues(string tag, DateTime start, DateTime end, uint count = 1, bool containBound = false)
        {
            HistoryReadValueId m_nodeToContinue = new()
            {
                NodeId = new NodeId(tag),
            };

            ReadRawModifiedDetails m_details = new()
            {
                StartTime = start,
                EndTime = end,
                NumValuesPerNode = count,
                IsReadModified = false,
                ReturnBounds = containBound
            };

            HistoryReadValueIdCollection nodesToRead = [m_nodeToContinue];


            m_session.HistoryRead(
                null,
                new ExtensionObject(m_details),
                TimestampsToReturn.Both,
                false,
                nodesToRead,
                out HistoryReadResultCollection results,
                out DiagnosticInfoCollection diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

            if (StatusCode.IsBad(results[0].StatusCode))
            {
                throw new ServiceResultException(results[0].StatusCode);
            }

            HistoryData? values = ExtensionObject.ToEncodeable(results[0].HistoryData) as HistoryData;
            foreach (var value in values.DataValues)
            {
                yield return value;
            }
        }

        /// <summary>
        /// 读取一连串的历史数据，并将其转化成指定的类型
        /// </summary>
        /// <param name="tag">节点的索引</param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="count">读取的个数</param>
        /// <param name="containBound">是否包含边界</param>
        /// <returns>读取的数据列表</returns>
        public IEnumerable<T>? ReadHistoryRawDataValues<T>(string tag, DateTime start, DateTime end, uint count = 1, bool containBound = false)
        {
            HistoryReadValueId m_nodeToContinue = new HistoryReadValueId()
            {
                NodeId = new NodeId(tag),
            };

            ReadRawModifiedDetails m_details = new()
            {
                StartTime = start.ToUniversalTime(),
                EndTime = end.ToUniversalTime(),
                NumValuesPerNode = count,
                IsReadModified = false,
                ReturnBounds = containBound
            };

            HistoryReadValueIdCollection nodesToRead = [m_nodeToContinue];


            m_session.HistoryRead(
                null,
                new ExtensionObject(m_details),
                TimestampsToReturn.Both,
                false,
                nodesToRead,
                out HistoryReadResultCollection results,
                out DiagnosticInfoCollection diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

            if (StatusCode.IsBad(results[0].StatusCode))
            {
                throw new ServiceResultException(results[0].StatusCode);
            }

            HistoryData? values = ExtensionObject.ToEncodeable(results[0].HistoryData) as HistoryData;
            return (values?.DataValues)?.Select(value => (T)value.Value);
        }

        #endregion

        #region BrowseNode Support

        /// <summary>
        /// 浏览一个节点的引用
        /// </summary>
        /// <param name="tag">节点值</param>
        /// <returns>引用节点描述</returns>
        public ReferenceDescription[]? BrowseNodeReference(string tag)
        {
            NodeId sourceId = new(tag);

            // 该节点可以读取到方法
            BrowseDescription nodeToBrowse1 = new()
            {
                NodeId = sourceId,
                BrowseDirection = BrowseDirection.Forward,
                ReferenceTypeId = ReferenceTypeIds.Aggregates,
                IncludeSubtypes = true,
                NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method),
                ResultMask = (uint)BrowseResultMask.All
            };

            // 该节点无论怎么样都读取不到方法
            // find all nodes organized by the node.
            BrowseDescription nodeToBrowse2 = new()
            {
                NodeId = sourceId,
                BrowseDirection = BrowseDirection.Forward,
                ReferenceTypeId = ReferenceTypeIds.Organizes,
                IncludeSubtypes = true,
                NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable),
                ResultMask = (uint)BrowseResultMask.All
            };

            BrowseDescriptionCollection nodesToBrowse = [nodeToBrowse1, nodeToBrowse2];



            return null;
        }

        #endregion

        #region Read Attributes Support


        /// <summary>
        /// 读取一个节点的所有属性
        /// </summary>
        /// <param name="tag">节点值</param>
        /// <returns>所有的数据</returns>
        public DataValue[]? ReadNoteDataValueAttributes(string tag)
        {
            NodeId sourceId = new NodeId(tag);
            ReadValueIdCollection nodesToRead = [];

            // attempt to read all possible attributes.
            // 尝试着去读取所有可能的特性
            for (uint ii = Attributes.NodeId; ii <= Attributes.UserExecutable; ii++)
            {
                ReadValueId nodeToRead = new()
                {
                    NodeId = sourceId,
                    AttributeId = ii
                };
                nodesToRead.Add(nodeToRead);
            }

            int startOfProperties = nodesToRead.Count;

            // find all of the pror of the node.
            BrowseDescription nodeToBrowse1 = new()
            {
                NodeId = sourceId,
                BrowseDirection = BrowseDirection.Forward,
                ReferenceTypeId = ReferenceTypeIds.HasProperty,
                IncludeSubtypes = true,
                NodeClassMask = 0,
                ResultMask = (uint)BrowseResultMask.All
            };

            BrowseDescriptionCollection nodesToBrowse = [nodeToBrowse1];

            // fetch property references from the server.
            //ReferenceDescriptionCollection references = FormUtils.Browse( m_session, nodesToBrowse, false );

            //if (references == null)
            //{
            //    return new DataValue[0];
            //}

            //for (int ii = 0; ii < references.Count; ii++)
            //{
            //    // ignore external references.
            //    if (references[ii].NodeId.IsAbsolute)
            //    {
            //        continue;
            //    }

            //    ReadValueId nodeToRead = new ReadValueId( );
            //    nodeToRead.NodeId = (NodeId)references[ii].NodeId;
            //    nodeToRead.AttributeId = Attributes.Value;
            //    nodesToRead.Add( nodeToRead );
            //}

            // read all values.
            DataValueCollection? results = null;
            DiagnosticInfoCollection? diagnosticInfos = null;

            m_session?.Read(
                null,
                0,
                TimestampsToReturn.Neither,
                nodesToRead,
                out results,
                out diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

            return results?.ToArray();
        }
        #endregion

        #region Method Call Support


        /// <summary>
        /// call a server method
        /// </summary>
        /// <param name="tagParent">方法的父节点tag</param>
        /// <param name="tag">方法的节点tag</param>
        /// <param name="args">传递的参数</param>
        /// <returns>输出的结果值</returns>
        public object[]? CallMethodByNodeId(string tagParent, string tag, params object[] args)
        {
            if (m_session == null)
            {
                return null;
            }

            IList<object> outputArguments = m_session.Call(
                new NodeId(tagParent),
                new NodeId(tag),
                args);

            return [.. outputArguments];
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Raises the connect complete event on the main GUI thread.
        /// </summary>
        private void DoConnectComplete(object? state)
        {
            m_ConnectComplete?.Invoke(this, EventArgs.Empty);
        }



        private void CheckReturnValue(StatusCode status)
        {
            if (!StatusCode.IsGood(status))
                throw new Exception(string.Format("Invalid response from the server. (Response Status: {0})", status));
        }

        #endregion

        #region Private Fields
        //客户端
        private ApplicationInstance application { get; set; }
        /// <summary>
        /// 客户端配置
        /// </summary>
        private ApplicationConfiguration m_configuration { get; set; }

        /// <summary>
        /// 会话 连接核心
        /// </summary>
        private Session m_session;
        private bool m_IsConnected;                       //是否已经连接过
        private int m_reconnectPeriod = 10;               // 重连状态
        private bool m_useSecurity;
        private bool m_ConnectedState = false;
        private SessionReconnectHandler? m_reconnectHandler;
        private EventHandler? m_ReconnectComplete;
        private EventHandler? m_ReconnectStarting;
        private EventHandler? m_KeepAliveComplete;
        private EventHandler? m_ConnectComplete;
        private EventHandler<OpcUaStatusEventArgs>? m_OpcStatusChange;

        private Dictionary<string, Subscription> dic_subscriptions = new Dictionary<string, Subscription>();        // 系统所有的节点信息
        #endregion


        public Dictionary<string, string> ReadTagTreeList(NodeId nodeId)
        {
            try
            {
                ReferenceDescriptionCollection references;
                Byte[] continuationPoint;

                if (m_session != null)
                {
                    m_session.Browse(
                    null,
                    null,
                    nodeId,
                    0u,
                    BrowseDirection.Forward,
                    ReferenceTypeIds.HierarchicalReferences,
                    true,
                    (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method,
                    out continuationPoint,
                    out references);
                    foreach (var rd in references)
                    {
                        if (rd.BrowseName.ToString().Contains("_") || rd.BrowseName.ToString() == "Server")
                            continue;
                        Console.WriteLine($"BrowseName: {rd.BrowseName}, NodeId: {rd.NodeId}");

                        // 如果节点是变量节点，读取其值
                        if (rd.NodeClass == NodeClass.Variable)
                        {
                            try
                            {
                                var value = m_session.ReadValue(rd.NodeId.ToString());
                                if (!NodeIds.ContainsKey(rd.NodeId.ToString()))
                                {
                                    NodeIds.Add(rd.BrowseName.ToString(), rd.NodeId.ToString());
                                }
                                Console.WriteLine($"Value: {value}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error reading value: {ex.Message}");
                            }
                        }
                        else
                        {
                            ReadTagTreeList(rd.NodeId.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return NodeIds;
        }

        /// <summary>
        /// 连接状态
        /// </summary>
        public bool ConnectedState
        {
            get => m_ConnectedState;
            set => Set(value, ref m_ConnectedState);
        }

        private void Set<T>(T newval, ref T oldval)
        {
            if (!EqualityComparer<T>.Default.Equals(newval, oldval))
            {
                oldval = newval;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Dictionary<string, string> NodeIds { get; set; } = new Dictionary<string, string>();
    }
}