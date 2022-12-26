namespace Data.LiteratureTime.Infrastructure;

using Irrbloss.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

public class ServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection service, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis");
        if (connectionString == null)
        {
            throw new Exception();
        }

        service.AddSingleton<IConnectionMultiplexer>(c =>
        {
            return ConnectionMultiplexer.Connect(connectionString);
        });

        service.AddTransient<
            Core.Interfaces.v2.ILiteratureProvider,
            Providers.v2.LiteratureProvider
        >();

        service.AddTransient<Core.Interfaces.v2.ICacheProvider, Providers.v2.CacheProvider>();
    }
}
