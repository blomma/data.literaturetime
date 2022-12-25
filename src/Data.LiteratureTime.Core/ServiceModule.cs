namespace Data.LiteratureTime.Core;

using Irrbloss.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

public class ServiceModule : IServiceModule
{
    public void AddServices(IServiceCollection service, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis");
        if (connectionString == null)
        {
            throw new Exception();
        }

        service.AddSingleton<IConnectionMultiplexer>(c =>
        {
            return ConnectionMultiplexer.Connect(connectionString);
        });

        service.AddSingleton<Workers.v2.LiteratureDataWorker>();
        service.AddTransient<Interfaces.v2.ILiteratureService, Services.v2.LiteratureService>();
    }
}
