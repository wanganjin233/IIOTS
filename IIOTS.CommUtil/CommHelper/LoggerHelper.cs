using Microsoft.Extensions.Logging; 
using IIOTS.Models; 

namespace IIOTS.CommUtil
{
    public class LoggerHelper(Action<LogInfo> SendAction) : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new MyLogger(categoryName, SendAction);
        }

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
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
