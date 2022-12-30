namespace Data.LiteratureTime.Infrastructure.Providers;

using Data.LiteratureTime.Core.Interfaces;
using StackExchange.Redis;

public class BusProvider : IBusProvider
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public BusProvider(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task PublishAsync(string channel, string message)
    {
        var db = _connectionMultiplexer.GetDatabase();
        _ = await db.PublishAsync(channel, message);
    }
}
