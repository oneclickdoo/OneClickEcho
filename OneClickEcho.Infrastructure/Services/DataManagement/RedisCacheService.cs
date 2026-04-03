using OneClickEcho.Application.Common.Services;
using StackExchange.Redis;
using System.Text.Json;

namespace OneClickEcho.Infrastructure.Services.DataManagement
{
    public class RedisCacheService(IConnectionMultiplexer redis) : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis = redis;

        public async Task SetCacheValueAsync<T>(string key, T value, TimeSpan expiration)
        {
            IDatabase db = _redis.GetDatabase();

            string json = JsonSerializer.Serialize(value);

            await db.StringSetAsync(key, json, expiration);
        }

        public async Task<T> GetCacheValueAsync<T>(string key)
        {
            IDatabase db = _redis.GetDatabase();

            RedisValue json = await db.StringGetAsync(key);

#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument.
            return json.HasValue ? JsonSerializer.Deserialize<T>(json) : default;
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
