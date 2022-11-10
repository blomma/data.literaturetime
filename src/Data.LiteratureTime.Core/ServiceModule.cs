namespace Data.LiteratureTime.Core;

using Irrbloss;
using Irrbloss.Interfaces;
using Microsoft.Extensions.DependencyInjection;

public class ServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection service)
    {
        service.AddSingleton<Workers.v2.LiteratureDataWorker>();
        service.AddSingleton<RedisConnection>();
        service.AddTransient<Interfaces.v2.ILiteratureService, Services.v2.LiteratureService>();
    }
}
