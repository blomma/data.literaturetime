namespace Data.LiteratureTime.Core;

using Data.LiteratureTime.Core.Workers.v3;
using Irrbloss.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class ServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection service, IConfiguration configuration)
    {
        service.AddTransient<Interfaces.ILiteratureService, Services.LiteratureService>();

        service.AddHostedService<LiteratureDataWorker>();
    }
}
