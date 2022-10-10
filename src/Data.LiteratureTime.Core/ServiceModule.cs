namespace Data.LiteratureTime.Core;

using Data.LiteratureTime.Core.Interfaces;
using Data.LiteratureTime.Core.Services;
using Data.LiteratureTime.Core.Workers;
using Irrbloss.Interfaces;
using Microsoft.Extensions.DependencyInjection;

public class ServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection service)
    {
        service.AddSingleton<LiteratureDataWorker>();
        service.AddSingleton<RedisConnection>();
        service.AddTransient<ILiteratureService, LiteratureService>();
    }
}
