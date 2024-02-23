using IIOTS.Enum;
using IIOTS.Models;
using System.Collections.Concurrent;
using System.Reflection;

namespace IIOTS.Util
{
    public class InstanceMethodInfo
    {
        /// <summary>
        /// 实列
        /// </summary>
        public object? Instance { get; set; }
        /// <summary>
        /// 方法
        /// </summary>
        public MethodInfo? MethodInfo { get; set; }
    }
    public class Handler<T>
    {
        /// <summary>
        /// 方法路由表
        /// </summary>
        public ConcurrentDictionary<string, InstanceMethodInfo> Routes = new();
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="objects"></param>
        public Handler(string identifier, params object[] objects)
        {
            foreach (var type in Assembly.GetCallingAssembly().GetTypes().Where(p => p.GetInterface(typeof(T).Name) != null))
            {
                List<object> parameters = [];
                //获取构造函数参数
                var InterfaceParameters = type.GetConstructors().FirstOrDefault()?.GetParameters();
                if (InterfaceParameters != null)
                {
                    //遍历构造函数参数
                    foreach (var InterfaceParameter in InterfaceParameters)
                    {
                        //匹配传入参数
                        var parameter = objects.FirstOrDefault(p => p.GetType().IsAssignableTo(InterfaceParameter.ParameterType));
                        if (parameter != null)
                        {
                            parameters.Add(parameter);
                        }
                        else
                        {
                            throw new Exception("无类型需要参数");
                        }
                    }
                }
                //创建实例
                object? instance = Activator.CreateInstance(type, parameters.ToArray(), null);
                //添加路由方法
                foreach (var methodInfo in type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(p => p.DeclaringType == type))
                {
                    Routes.TryAdd(string.Join("/", [identifier, type.Name.Replace("Handler", ""), methodInfo.Name])
                        , new InstanceMethodInfo { Instance = instance, MethodInfo = methodInfo });
                }
            }
        }
        /// <summary>
        /// 方法调用并返回结果
        /// </summary>
        /// <param name="router"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private object? MethodInfoInvoke(string router, string data)
        {
            //根据路由获取实列方法
            Routes.TryGetValue(router, out var instance);
            if (instance?.MethodInfo != null)
            {
                //获取方法参数0
                var parameter = instance.MethodInfo.GetParameters().FirstOrDefault();
                if (parameter != null)
                {
                    object? dataObject = data.ToObject(parameter.ParameterType);
                    return instance.MethodInfo.Invoke(instance.Instance, [dataObject]);
                }
                else
                {
                    return instance.MethodInfo.Invoke(instance.Instance, null);
                }
            }
            else
            {
                throw new Exception($"路由【{router}】不存在");
            }
        }
        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="router"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public HandlerResult ExecuteHandler(string router, string data)
        {
            router = router.Split(";").First();
            string[] routes = router.Split("/");
            string flag = routes.SkipLast(1).Last();
            return flag switch
            {
                //请求需要回复
                "Request" => new HandlerResult()
                {
                    MsgType = MsgTypeEnum.Request,
                    MsgCode = routes.Last(),
                    Router = string.Join("/", routes
                             .TakeLast(3).SkipLast(2)
                             .Append("Response")
                             .Append(routes.Last())),
                    Data = MethodInfoInvoke(string.Join("/", routes.SkipLast(3)), data)
                },
                //接收到回应结果
                "Response" => new HandlerResult()
                {
                    MsgType = MsgTypeEnum.Response,
                    MsgCode = routes.Last(),
                    Data = data
                },
                //执行
                _ => new HandlerResult()
                {
                    MsgType = MsgTypeEnum.Execute,
                    Data = MethodInfoInvoke(router, data)
                }
            };
        }
    }
}
