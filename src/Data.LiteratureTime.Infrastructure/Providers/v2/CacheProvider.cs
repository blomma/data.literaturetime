namespace Data.LiteratureTime.Infrastructure.Providers.v2;

using System.Text.Json;
using Data.LiteratureTime.Core.Interfaces.v2;
using StackExchange.Redis;

public class CacheProvider : ICacheProvider
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public CacheProvider(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task Set<T>(string key, T data, TimeSpan? expiration = null)
    {
        var db = _connectionMultiplexer.GetDatabase();
        var jsonData = JsonSerializer.Serialize(data);

        await db.StringSetAsync(key, jsonData, expiration);
    }

    public async Task<bool> Exists(string key)
    {
        var db = _connectionMultiplexer.GetDatabase();

        return await db.KeyExistsAsync(key);
    }
}
