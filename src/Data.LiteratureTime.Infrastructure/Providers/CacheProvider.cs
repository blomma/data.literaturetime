namespace Data.LiteratureTime.Infrastructure.Providers;

using System.Text.Json;
using Data.LiteratureTime.Core.Interfaces;
using StackExchange.Redis;

public class CacheProvider(IConnectionMultiplexer connectionMultiplexer) : ICacheProvider
{
    private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer;

    public Task<bool> SetAsync<T>(string key, T data, TimeSpan? expiration = null)
    {
        var db = _connectionMultiplexer.GetDatabase();
        var jsonData = JsonSerializer.Serialize(data);

        return db.StringSetAsync(key, jsonData, expiration);
    }

    public Task<bool> ExistsAsync(string key)
    {
        var db = _connectionMultiplexer.GetDatabase();

        return db.KeyExistsAsync(key);
    }
}
