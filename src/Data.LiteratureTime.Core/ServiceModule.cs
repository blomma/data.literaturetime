namespace Data.LiteratureTime.Core;

using Irrbloss;
using Irrbloss.Interfaces;
using Microsoft.Extensions.DependencyInjection;

public class ServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection service)
    {
        service.AddSingleton<Workers.v1.LiteratureDataWorker>();
        service.AddSingleton<RedisConnection>();
        service.AddTransient<Interfaces.v1.ILiteratureService, Services.v1.LiteratureService>();
    }
}
