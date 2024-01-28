using System.Text.Json;
using Data.LiteratureTime.Core.Exceptions;
using Data.LiteratureTime.Core.Interfaces;
using StackExchange.Redis;

namespace Data.LiteratureTime.Infrastructure.Providers;

public class CacheProvider(IConnectionMultiplexer connectionMultiplexer) : ICacheProvider
{
    public async Task SetAsync<T>(string key, T data, TimeSpan? expiration = null)
    {
        var jsonData = JsonSerializer.Serialize(data);

        var db = connectionMultiplexer.GetDatabase();
        if (!await db.StringSetAsync(key, jsonData, expiration))
        {
            throw new CacheException($"Unable to set key:{key}");
        }
    }

    public async Task SetAsync<T>(string key, List<T> data, TimeSpan? expiration = null)
    {
        var jsonData = data.Select(d => JsonSerializer.Serialize(d))
            .Select(d => new RedisValue(d))
            .ToArray();

        var db = connectionMultiplexer.GetDatabase();
        await db.KeyDeleteAsync(key);

        await db.SetAddAsync(key, jsonData);

        if (expiration != null)
        {
            if (!await db.KeyExpireAsync(key, expiration))
            {
                throw new CacheException($"Unable to set expiration:{expiration} on key:{key}");
            }
        }
    }

    public Task<bool> ExistsAsync(string key)
    {
        var db = connectionMultiplexer.GetDatabase();
        return db.KeyExistsAsync(key);
    }
}
