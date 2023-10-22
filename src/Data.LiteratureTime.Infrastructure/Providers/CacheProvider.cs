namespace Data.LiteratureTime.Infrastructure.Providers;

using System.Text.Json;
using Data.LiteratureTime.Core.Interfaces;
using StackExchange.Redis;

public class CacheProvider(IConnectionMultiplexer connectionMultiplexer) : ICacheProvider
{
    public Task<bool> SetAsync<T>(string key, T data, TimeSpan? expiration = null)
    {
        var jsonData = JsonSerializer.Serialize(data);

        var db = connectionMultiplexer.GetDatabase();
        return db.StringSetAsync(key, jsonData, expiration);
    }

    public Task<bool> ExistsAsync(string key)
    {
        var db = connectionMultiplexer.GetDatabase();
        return db.KeyExistsAsync(key);
    }
}
