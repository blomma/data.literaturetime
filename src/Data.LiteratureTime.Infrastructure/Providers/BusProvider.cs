namespace Data.LiteratureTime.Infrastructure.Providers;

using Data.LiteratureTime.Core.Interfaces;
using StackExchange.Redis;

public class BusProvider(IConnectionMultiplexer connectionMultiplexer) : IBusProvider
{
    public async Task PublishAsync(string channel, string message)
    {
        var db = connectionMultiplexer.GetDatabase();
        var redisChannel = RedisChannel.Literal(channel);

        _ = await db.PublishAsync(redisChannel, message);
    }
}
