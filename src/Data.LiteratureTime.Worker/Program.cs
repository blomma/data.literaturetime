using System.Globalization;
using Irrbloss.Extensions;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        LogEventLevel.Verbose,
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        CultureInfo.CurrentCulture
    )
    .CreateBootstrapLogger();

var builder = Host.CreateDefaultBuilder(args);
builder.UseSerilog(
    (context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services)
);

builder.ConfigureServices(
    (context, services) =>
    {
        services.AddServiceModules(context.Configuration);
    }
);

var host = builder.Build();
host.Run();
