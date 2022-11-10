namespace Data.LiteratureTime.Core;

using Irrbloss;
using Irrbloss.Interfaces;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class StartupModule : IStartupModule
{
    public void AddStartups(IEndpointRouteBuilder app)
    {
        IConfiguration configuration = app.ServiceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("Redis");

        if (connectionString == null) {
            throw new Exception();
        }

        var redisConnection = app.ServiceProvider.GetRequiredService<RedisConnection>();
        redisConnection.Initalize(connectionString);

        var literatureDataWorker2 =
            app.ServiceProvider.GetRequiredService<Workers.v2.LiteratureDataWorker>();
        literatureDataWorker2.Run();
    }
}
