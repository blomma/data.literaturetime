namespace Data.LiteratureTime.Core;

using Irrbloss.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class ServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection service, IConfiguration configuration)
    {
        service.AddSingleton<Workers.v2.LiteratureDataWorker>();
        service.AddTransient<Interfaces.v2.ILiteratureService, Services.v2.LiteratureService>();
    }
}
