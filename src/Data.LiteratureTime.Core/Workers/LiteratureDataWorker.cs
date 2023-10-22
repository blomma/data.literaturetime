using System.Collections.Concurrent;

namespace Data.LiteratureTime.Core.Workers;

using System.Threading.Tasks;
using Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;

public class LiteratureDataWorker(
    ILogger<LiteratureDataWorker> logger,
    IServiceProvider serviceProvider
) : BackgroundService
{
    private const string KeyPrefix = "LIT_V4";
    private const string IndexMarker = "INDEX";

    private static string PrefixKey(string key) => $"{KeyPrefix}:{key}";

    private async Task PopulateAsync()
    {
        logger.LogInformation("Repopulating cache");

        using var scope = serviceProvider.CreateScope();
        var cacheProvider = scope.ServiceProvider.GetRequiredService<ICacheProvider>();
        var literatureService = scope.ServiceProvider.GetRequiredService<ILiteratureService>();

        var literatureTimes = literatureService.GetLiteratureTimes();

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 3 };
        var exceptions = new ConcurrentQueue<Exception>();
        await Parallel.ForEachAsync(
            literatureTimes,
            parallelOptions,
            async (literatureTime, _) =>
            {
                try
                {
                    var key = PrefixKey(literatureTime.Hash);
                    var success = await cacheProvider.SetAsync(
                        key,
                        literatureTime,
                        TimeSpan.FromHours(2)
                    );
                    if (!success)
                    {
                        throw new Exception(
                            $"Unable to save literature time with key:{key} to cache"
                        );
                    }
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

        var grouped = literatureTimes.Select(s => new LiteratureTimeIndex(s.Time, s.Hash));
        var indexKey = PrefixKey(IndexMarker);
        var success = await cacheProvider.SetAsync(indexKey, grouped, TimeSpan.FromHours(2));
        if (!success)
        {
            throw new Exception($"Unable to save index with key:{indexKey} to cache");
        }

        var busProvider = scope.ServiceProvider.GetRequiredService<IBusProvider>();
        await busProvider.PublishAsync("literature", PrefixKey("index"));

        logger.LogInformation("Done repopulating cache");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            PeriodicTimer timer = new(TimeSpan.FromSeconds(5));

            while (
                !stoppingToken.IsCancellationRequested
                && await timer.WaitForNextTickAsync(stoppingToken)
            )
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var cacheProvider = scope.ServiceProvider.GetRequiredService<ICacheProvider>();
                    var indexKey = PrefixKey(IndexMarker);
                    var keyExists = await cacheProvider.ExistsAsync(indexKey);
                    if (keyExists)
                    {
                        continue;
                    }

                    logger.LogInformation("Index not found");
                    await PopulateAsync();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Caught exception");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // When the stopping token is canceled, for example, a call made from services.msc,
            // we shouldn't exit with a non-zero exit code. In other words, this is expected...
        }
    }
}
