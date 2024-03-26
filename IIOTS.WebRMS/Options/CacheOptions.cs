namespace IIOTS.WebRMS
{
    internal class CacheOptions
    {
        public CacheType CacheType { get; set; }
        public string? RedisEndpoint { get; set; }
    }
}
