namespace Data.LiteratureTime.Core;

using Irrbloss.Interfaces;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

public class StartupModule : IStartupModule
{
    public void AddStartups(IEndpointRouteBuilder app)
    {
        var literatureDataWorker2 =
            app.ServiceProvider.GetRequiredService<Workers.v2.LiteratureDataWorker>();
        literatureDataWorker2.Run();

        var literatureDataWorker3 =
            app.ServiceProvider.GetRequiredService<Workers.v3.LiteratureDataWorker>();
        literatureDataWorker3.Run();
    }
}
