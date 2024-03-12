using System.Collections.Concurrent;
using Data.LiteratureTime.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Data.LiteratureTime.Core.Workers;

internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Information, "Cache `{message}`")]
    public static partial void Cache(this ILogger logger, string message);

    [LoggerMessage(LogLevel.Error, "Cache `{message}`")]
    public static partial void Cache(this ILogger logger, Exception ex, string message);
}

public class LiteratureDataWorker(
    ILogger<LiteratureDataWorker> logger,
    IServiceProvider serviceProvider
) : BackgroundService
{
    private static string PrefixKey(string key) => $"literature:time:{key}";

    private async Task PopulateAsync()
    {
        logger.Cache("Start populating");

        using var scope = serviceProvider.CreateScope();
        var cacheProvider = scope.ServiceProvider.GetRequiredService<ICacheProvider>();
        var literatureService = scope.ServiceProvider.GetRequiredService<ILiteratureService>();

        var literatureTimes = literatureService.GetLiteratureTimes();

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 3 };
        var exceptions = new ConcurrentQueue<Exception>();

        var groupedLiteratureTimes = literatureTimes.GroupBy(l => l.Time);

        await Parallel.ForEachAsync(
            groupedLiteratureTimes,
            parallelOptions,
            async (literatureTime, _) =>
            {
                try
                {
                    var key = PrefixKey(literatureTime.Key);
                    await cacheProvider.SetAsync(
                        key,
                        literatureTime.ToList(),
                        TimeSpan.FromDays(3)
                    );
                }
                catch (Exception e)
                {
                    exceptions.Enqueue(e);
                }
            }
        );

        if (!exceptions.IsEmpty)
        {
            throw new AggregateException(exceptions);
        }

        var key = PrefixKey("marker");
        await cacheProvider.SetAsync(key, string.Empty, TimeSpan.FromDays(2));

        logger.Cache("Done populating");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await PopulateAsync();

            PeriodicTimer timer = new(TimeSpan.FromSeconds(5));

            while (
                !stoppingToken.IsCancellationRequested
                && await timer.WaitForNextTickAsync(stoppingToken)
            )
            {
                using var scope = serviceProvider.CreateScope();

                try
                {
                    var cacheProvider = scope.ServiceProvider.GetRequiredService<ICacheProvider>();
                    var indexKey = PrefixKey("marker");
                    var keyExists = await cacheProvider.ExistsAsync(indexKey);
                    if (keyExists)
                    {
                        continue;
                    }

                    logger.Cache("Marker not found");

                    await PopulateAsync();
                }
                catch (Exception e)
                {
                    logger.Cache(e, e.Message);
                }
            }

            timer.Dispose();
        }
        catch (OperationCanceledException)
        {
            // When the stopping token is canceled, for example, a call made from services.msc,
            // we shouldn't exit with a non-zero exit code. In other words, this is expected...
        }
    }
}
