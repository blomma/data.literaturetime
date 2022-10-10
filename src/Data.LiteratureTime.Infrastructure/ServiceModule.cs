namespace Data.LiteratureTime.Infrastructure;

using Data.LiteratureTime.Core.Interfaces;
using Data.LiteratureTime.Infrastructure.Providers;
using Irrbloss.Interfaces;
using Microsoft.Extensions.DependencyInjection;

public class ServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection service)
    {
        service.AddTransient<ILiteratureProvider, LiteratureProvider>();
    }
}
