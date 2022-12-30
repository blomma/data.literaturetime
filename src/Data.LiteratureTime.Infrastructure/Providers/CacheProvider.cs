namespace Data.LiteratureTime.Infrastructure.Providers;

using System.Text.Json;
using Data.LiteratureTime.Core.Interfaces;
using StackExchange.Redis;

public class CacheProvider : ICacheProvider
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public CacheProvider(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public Task<bool> SetAsync<T>(string key, T data, TimeSpan? expiration = null)
    {
        var db = _connectionMultiplexer.GetDatabase();
        var jsonData = JsonSerializer.Serialize(data);

        return db.StringSetAsync(key, jsonData, expiration);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        var db = _connectionMultiplexer.GetDatabase();

        return await db.KeyExistsAsync(key);
    }
}
