namespace Data.LiteratureTime.Infrastructure;

using Irrbloss.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class ServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection service, IConfiguration configuration)
    {
        service.AddTransient<
            Core.Interfaces.v2.ILiteratureProvider,
            Providers.v2.LiteratureProvider
        >();

        service.AddTransient<Core.Interfaces.v2.ICacheProvider, Providers.v2.CacheProvider>();
    }
}
