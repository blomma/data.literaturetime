namespace Data.LiteratureTime.Core;

using Irrbloss;
using Irrbloss.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class ServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection service, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis");
        if (connectionString == null)
        {
            throw new Exception();
        }

        service.AddSingleton<Workers.v2.LiteratureDataWorker>();
        service.AddSingleton(s =>
        {
            return new RedisConnection(connectionString);
        });

        service.AddTransient<Interfaces.v2.ILiteratureService, Services.v2.LiteratureService>();
    }
}
