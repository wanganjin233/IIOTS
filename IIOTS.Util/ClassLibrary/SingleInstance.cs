namespace IIOTS.Util
{
    public class SingleInstance<T> where T : class, new()
    {
        private static volatile T? _Value = default;
        private static object _lock = new object();
        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    return _Value ??= new T();
                }
            }
        }
    }
}
