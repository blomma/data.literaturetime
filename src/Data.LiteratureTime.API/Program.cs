using Irrbloss.Extensions;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration().MinimumLevel
    .Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services)
);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddMvcCore();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddServiceModules(builder.Configuration);
builder.Services.AddRouterModules();

builder.Services.AddManagedResponseException();

builder.Services.AddHttpLogging(logging =>
{
    logging.RequestHeaders.Add("Referer");
    logging.RequestHeaders.Add("X-Forwarded-For");
    logging.RequestHeaders.Add("X-Forwarded-Host");
    logging.RequestHeaders.Add("X-Forwarded-Port");
    logging.RequestHeaders.Add("X-Forwarded-Proto");
    logging.RequestHeaders.Add("X-Forwarded-Server");
    logging.RequestHeaders.Add("X-Real-Ip");
    logging.RequestHeaders.Add("Upgrade-Insecure-Requests");
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});

var app = builder.Build();
app.UseHttpLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseManagedResponseException();
app.UseRouterModules();
app.UseStartupModules();

app.Run();
