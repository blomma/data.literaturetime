namespace Data.LiteratureTime.Core;

using Data.LiteratureTime.Core.Workers;
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

        var literatureDataWorker = app.ServiceProvider.GetRequiredService<LiteratureDataWorker>();
        literatureDataWorker.Run();
    }
}
