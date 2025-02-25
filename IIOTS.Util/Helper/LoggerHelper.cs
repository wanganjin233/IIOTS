using Microsoft.Extensions.Logging;  

namespace IIOTS.Util
{
    public class LogInfo
    {
        /// <summary>
        /// 种类名称
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
        /// <summary>
        /// 日志级别
        /// </summary>
        public int LogLevel { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public object? State { get; set; }
        /// <summary>
        /// 日志内容
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
    public class LoggerHelper(Action<LogInfo> SendAction) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new MyLogger(categoryName, SendAction);
        }

        public void Dispose()
        {
        }
        class MyLogger : ILogger
        {
            private readonly string _CategoryName;
            public MyLogger(string categoryName, Action<LogInfo> SendAction)
            {
                _SendAction = SendAction;
                _CategoryName = categoryName;
            }
            private readonly Action<LogInfo> _SendAction;
           
            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                _SendAction?.Invoke(new LogInfo()
                {
                    CategoryName = _CategoryName,
                    LogLevel = (int)logLevel,
                    Message = formatter.Invoke(state, exception),
                    State = state
                });
            }

            class Disposable : IDisposable
            {
                public void Dispose()
                {

                }
            }
            public IDisposable BeginScope<TState>(TState state) where TState : notnull
            {
                return new Disposable();
            }
        }
    }
}
