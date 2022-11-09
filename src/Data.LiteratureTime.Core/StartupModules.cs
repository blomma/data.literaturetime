namespace Data.LiteratureTime.Core;

using Irrbloss;
using Irrbloss.Interfaces;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

public class StartupModule : IStartupModule
{
    public void AddStartups(IEndpointRouteBuilder app)
    {
        var redisConnection = app.ServiceProvider.GetRequiredService<RedisConnection>();
        redisConnection.Initalize();

        var literatureDataWorker =
            app.ServiceProvider.GetRequiredService<Workers.v1.LiteratureDataWorker>();
        literatureDataWorker.Run();

        var literatureDataWorker2 =
            app.ServiceProvider.GetRequiredService<Workers.v2.LiteratureDataWorker>();
        literatureDataWorker2.Run();
    }
}
