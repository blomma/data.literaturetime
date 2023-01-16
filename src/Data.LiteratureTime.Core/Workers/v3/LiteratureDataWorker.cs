namespace Data.LiteratureTime.Core.Workers.v3;

using Data.LiteratureTime.Core.Interfaces;
using Data.LiteratureTime.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class LiteratureDataWorker : IHostedService
{
    private Timer? intervalTimer;
    private Timer? sanityCheckTimer;

    private readonly SemaphoreSlim _lockSemaphore = new(initialCount: 1, maxCount: 1);

    private const string KEY_PREFIX = "LIT_V3";
    private const string INDEXMARKER = "INDEX";

    private readonly ILogger<LiteratureDataWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public LiteratureDataWorker(
        ILogger<LiteratureDataWorker> logger,
        IServiceProvider serviceProvider
    )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    private static string PrefixKey(string key) => $"{KEY_PREFIX}:{key}";

    private async Task PopulateAsync()
    {
        _logger.LogInformation("Repopulating cache");

        using var scope = _serviceProvider.CreateScope();
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

        Task.WaitAll(tasks.ToArray());

        var grouped = literatureTimes.Select(s => new LiteratureTimeIndex(s.Time, s.Hash));
        var indexKey = PrefixKey(INDEXMARKER);
        var success = await cacheProvider.SetAsync(indexKey, grouped, TimeSpan.FromHours(2));
        if (!success)
        {
            throw new Exception("Unable to save index");
        }

        var busProvider = scope.ServiceProvider.GetRequiredService<IBusProvider>();
        await busProvider.PublishAsync("literature", "index");

        _logger.LogInformation("Done repopulating cache");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        intervalTimer = new Timer(
            async (timerState) =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    if (!await _lockSemaphore.WaitAsync(0))
                        return;
                }
                catch
                {
                    return;
                }

                try
                {
                    await PopulateAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Caught exception");
                }
                finally
                {
                    _lockSemaphore.Release();
                }
            },
            null,
            0,
            (int)TimeSpan.FromHours(1).TotalMilliseconds
        );

        sanityCheckTimer = new Timer(
            async (timerState) =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    if (!await _lockSemaphore.WaitAsync(0))
                        return;
                }
                catch
                {
                    return;
                }

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var cacheProvider = scope.ServiceProvider.GetRequiredService<ICacheProvider>();
                    var indexKey = PrefixKey(INDEXMARKER);
                    var keyExists = await cacheProvider.ExistsAsync(indexKey);
                    if (keyExists)
                        return;

                    _logger.LogInformation("Index not found, populating cache");

                    await PopulateAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Caught exception");
                }
                finally
                {
                    _lockSemaphore.Release();
                }
            },
            null,
            0,
            (int)TimeSpan.FromSeconds(5).TotalMilliseconds
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        intervalTimer?.Change(Timeout.Infinite, 0);
        sanityCheckTimer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }
}