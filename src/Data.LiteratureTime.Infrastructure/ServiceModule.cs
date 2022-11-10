namespace Data.LiteratureTime.Infrastructure;

using Irrbloss.Interfaces;
using Microsoft.Extensions.DependencyInjection;

public class ServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection service)
    {
        service.AddTransient<
            Core.Interfaces.v2.ILiteratureProvider,
            Providers.v2.LiteratureProvider
        >();
    }
}
