namespace Data.LiteratureTime.Infrastructure.Providers;

using Data.LiteratureTime.Core.Interfaces;
using StackExchange.Redis;

public class BusProvider(IConnectionMultiplexer connectionMultiplexer) : IBusProvider
{
    private readonly IConnectionMultiplexer _connectionMultiplexer = connectionMultiplexer;

    public async Task PublishAsync(string channel, string message)
    {
        var db = _connectionMultiplexer.GetDatabase();
        var redisChannel = RedisChannel.Literal(channel);

        _ = await db.PublishAsync(redisChannel, message);
    }
}
