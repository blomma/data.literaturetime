using Data.LiteratureTime.Core.Workers;
using Irrbloss.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Data.LiteratureTime.Core;

public class ServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection service, IConfiguration configuration)
    {
        service.AddTransient<Interfaces.ILiteratureService, Services.LiteratureService>();

        service.AddHostedService<LiteratureDataWorker>();
    }
}
