using System.Collections.Concurrent;
using Data.LiteratureTime.Core.Exceptions;
using Data.LiteratureTime.Core.Interfaces;
using Data.LiteratureTime.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Data.LiteratureTime.Core.Workers;

public static partial class LiteratureDataWorkerLog
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Cache:{message}")]
    public static partial void Cache(ILogger logger, string message);

    [LoggerMessage(EventId = 1, Message = "Index:{message}")]
    public static partial void Index(ILogger logger, LogLevel level, Exception? ex, string message);
}

public class LiteratureDataWorker(
    ILogger<LiteratureDataWorker> logger,
    IServiceProvider serviceProvider
) : BackgroundService
{
    private const string KeyPrefix = "LIT_V3";
    private const string IndexMarker = "INDEX";

    private static string PrefixKey(string key) => $"{KeyPrefix}:{key}";

    private async Task PopulateAsync()
    {
        LiteratureDataWorkerLog.Cache(logger, "Start populating");

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
                        TimeSpan.FromDays(3)
                    );

                    if (!success)
                    {
                        throw new CacheException(
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
        var success = await cacheProvider.SetAsync(indexKey, grouped, TimeSpan.FromDays(2));
        if (!success)
        {
            throw new CacheException($"Unable to save index with key:{indexKey} to cache");
        }

        var busProvider = scope.ServiceProvider.GetRequiredService<IBusProvider>();
        await busProvider.PublishAsync("literature", PrefixKey("index"));

        LiteratureDataWorkerLog.Cache(logger, "Done populating");
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

                    LiteratureDataWorkerLog.Index(logger, LogLevel.Information, null, "Not found");
                    await PopulateAsync();
                }
                catch (Exception e)
                {
                    LiteratureDataWorkerLog.Index(logger, LogLevel.Error, e, e.Message);
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
