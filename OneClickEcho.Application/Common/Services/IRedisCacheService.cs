namespace OneClickEcho.Application.Common.Services
{
    public interface IRedisCacheService
    {
        Task SetCacheValueAsync<T>(string key, T value, TimeSpan expiration);

        Task<T> GetCacheValueAsync<T>(string key);
    }
}
