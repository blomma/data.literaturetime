namespace Data.LiteratureTime.Core;

using Irrbloss.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class ServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection service, IConfiguration configuration)
    {
        service.AddSingleton<Workers.v3.LiteratureDataWorker>();
        service.AddTransient<Interfaces.ILiteratureService, Services.LiteratureService>();
    }
}
