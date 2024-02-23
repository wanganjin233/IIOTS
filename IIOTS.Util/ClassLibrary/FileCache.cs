namespace IIOTS.Util
{
    /// <summary>
    /// 缓存操作
    /// </summary>
    public class FileCache
    {
        /// <summary>
        /// 缓存路径
        /// </summary>
        private readonly string _cachePath;

        private readonly AutoResetEvent _CacheReset = new AutoResetEvent(false);
        private readonly UsingLock<object> UsingLock = new UsingLock<object>();
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="path"></param>
        public FileCache(string path)
        {
            string cachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
            if (!Directory.Exists(cachePath))
                Directory.CreateDirectory(cachePath);
            _cachePath = Path.Combine(cachePath, path);
            if (!Directory.Exists(_cachePath))
                Directory.CreateDirectory(_cachePath);
        }
        /// <summary>
        /// 获取缓存文件路径
        /// </summary>
        /// <param name="Read"></param>
        /// <param name="Desc"></param>
        /// <returns></returns>
        private string GetCachePath(bool Read, bool Desc)
        {
            string cacheFilePath = string.Empty;
            Dictionary<string, long> CacheFileTime = new Dictionary<string, long>();
            foreach (var CacheFile in Directory.GetFiles(_cachePath))
            {
                CacheFileTime.Add(CacheFile, long.Parse(Path.GetFileName(CacheFile)));
            }
            if (CacheFileTime.Any())
            {
                if (Desc)
                {
                    cacheFilePath = CacheFileTime.OrderByDescending(x => x.Value).First().Key;
                    if (File.ReadLines(cacheFilePath).Count() >= 1000)
                    {
                        cacheFilePath = Path.Combine(_cachePath, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
                    }
                }
                else
                {
                    cacheFilePath = CacheFileTime.OrderBy(x => x.Value).First().Key;
                }
            }
            else if (!Read)
            {
                cacheFilePath = Path.Combine(_cachePath, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
            }
            return cacheFilePath;
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <returns></returns>
        public string Read()
        {
            using (UsingLock.Write())
            {
                var createPath = Path.Combine(_cachePath, "cache");
                if (!File.Exists(createPath))
                {
                    File.CreateText(createPath).Close();
                }
                return File.ReadAllText(createPath);
            }
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="messager"></param>
        public void Write(string messager)
        {
            using (UsingLock.Write())
            {
                var createPath = Path.Combine(_cachePath, "cache");
                if (!File.Exists(createPath))
                {
                    File.CreateText(createPath).Close();
                }
                File.WriteAllText(createPath, messager);
            }
        }
        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="messager"></param>
        public void Enqueue(string messager)
        {
            using (UsingLock.Write())
            {
                string cachePath = GetCachePath(false, true);
                File.AppendAllLines(cachePath, new List<string> { messager });
                _CacheReset.Set();
            }
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <returns></returns>
        public string Dequeue()
        {
            while (true)
            {
                string? cacheLine = null;
                using (UsingLock.Write())
                {
                    string cachePath = GetCachePath(true, false);
                    if (!string.IsNullOrEmpty(cachePath))
                    {
                        cacheLine = File.ReadLines(cachePath).FirstOrDefault();
                    }
                }
                if (string.IsNullOrEmpty(cacheLine))
                {
                    _CacheReset.WaitOne();
                }
                else
                {
                    return cacheLine;
                }
            }
        }
        /// <summary>
        /// 确认
        /// </summary>
        public void ACK()
        {
            using (UsingLock.Write())
            {
                string cachePath = GetCachePath(true, false);
                if (!string.IsNullOrEmpty(cachePath))
                {
                    var lines = File.ReadAllLines(cachePath);
                    if (lines.Length > 1)
                    {
                        File.WriteAllLines(cachePath, lines.Skip(1));
                    }
                    else
                    {
                        File.Delete(cachePath);
                    }
                }
            }
        }
    }
}
