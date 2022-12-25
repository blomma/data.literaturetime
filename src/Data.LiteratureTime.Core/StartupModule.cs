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
        var literatureDataWorker2 =
            app.ServiceProvider.GetRequiredService<Workers.v2.LiteratureDataWorker>();
        literatureDataWorker2.Run();
    }
}
