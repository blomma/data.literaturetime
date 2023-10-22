namespace Data.LiteratureTime.Infrastructure;

using Irrbloss.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

public class ServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection service, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis") ?? throw new Exception();
        service.AddSingleton<IConnectionMultiplexer>(
            _ => ConnectionMultiplexer.Connect(connectionString)
        );

        service.AddTransient<Core.Interfaces.ILiteratureProvider, Providers.LiteratureProvider>();

        service.AddTransient<Core.Interfaces.ICacheProvider, Providers.CacheProvider>();
        service.AddTransient<Core.Interfaces.IBusProvider, Providers.BusProvider>();
    }
}
