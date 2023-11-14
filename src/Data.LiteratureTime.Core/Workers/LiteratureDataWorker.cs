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
) : IHostedService
{
    private Timer? _sanityCheckTimer;

    private readonly SemaphoreSlim _lockSemaphore = new(initialCount: 1, maxCount: 1);

    private const string KEY_PREFIX = "LIT_V3";
    private const string INDEXMARKER = "INDEX";

    private static string PrefixKey(string key) => $"{KEY_PREFIX}:{key}";

    private async Task PopulateAsync()
    {
        logger.LogInformation("Repopulating cache");

        using var scope = serviceProvider.CreateScope();
        var cacheProvider = scope.ServiceProvider.GetRequiredService<ICacheProvider>();
        var literatureService = scope.ServiceProvider.GetRequiredService<ILiteratureService>();

        var literatureTimes = await literatureService.GetLiteratureTimesAsync();
        var tasks = new List<Task>(literatureTimes.Count);

        foreach (var literatureTime in literatureTimes)
        {
            var key = PrefixKey(literatureTime.Hash);
            var task = cacheProvider.SetAsync(key, literatureTime, TimeSpan.FromHours(2));
            tasks.Add(task);
        }

        Task.WaitAll([ .. tasks ]);

        var grouped = literatureTimes.Select(s => new LiteratureTimeIndex(s.Time, s.Hash));
        var indexKey = PrefixKey(INDEXMARKER);
        var success = await cacheProvider.SetAsync(indexKey, grouped, TimeSpan.FromHours(2));
        if (!success)
        {
            throw new Exception("Unable to save index");
        }

        var busProvider = scope.ServiceProvider.GetRequiredService<IBusProvider>();
        await busProvider.PublishAsync("literature", "index");

        logger.LogInformation("Done repopulating cache");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        async void Callback(object? _)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                if (!await _lockSemaphore.WaitAsync(0, cancellationToken))
                    return;
            }
            catch
            {
                return;
            }

            try
            {
                using var scope = serviceProvider.CreateScope();
                var cacheProvider = scope.ServiceProvider.GetRequiredService<ICacheProvider>();
                var indexKey = PrefixKey(INDEXMARKER);
                var keyExists = await cacheProvider.ExistsAsync(indexKey);
                if (keyExists)
                    return;

                logger.LogInformation("Index not found, populating cache");

                await PopulateAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Caught exception");
            }
            finally
            {
                _lockSemaphore.Release();
            }
        }

        _sanityCheckTimer = new Timer(
            Callback,
            null,
            0,
            (int)TimeSpan.FromSeconds(5).TotalMilliseconds
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _sanityCheckTimer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }
}
