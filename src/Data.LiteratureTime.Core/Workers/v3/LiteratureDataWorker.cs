namespace Data.LiteratureTime.Core.Workers.v3;

using Data.LiteratureTime.Core.Interfaces.v2;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class LiteratureDataWorker
{
    private readonly ILiteratureService _literatureService;
    private Timer? intervalTimer;
    private Timer? sanityCheckTimer;

    private readonly SemaphoreSlim _lockSemaphore = new(initialCount: 1, maxCount: 1);

    private const string COMPLETETMARKER = "LITERATURE_COMPLETE_MARKER";
    private const string KEY_PREFIX = "LIT_V3";

    private readonly ILogger<LiteratureDataWorker> _logger;

    private readonly ICacheProvider _cacheProvider;

    public LiteratureDataWorker(
        ILiteratureService literatureService,
        ILogger<LiteratureDataWorker> logger,
        ICacheProvider cacheProvider
    )
    {
        _literatureService = literatureService;
        _logger = logger;
        _cacheProvider = cacheProvider;
    }

    private static string PrefixKey(string key) => $"{KEY_PREFIX}:{key}";

    private async Task PopulateAsync()
    {
        _logger.LogInformation("Repopulating cache");

        var literatureTimes = await _literatureService.GetLiteratureTimesAsync();
        var tasks = new List<Task>(literatureTimes.Count);

        foreach (var literatureTime in literatureTimes)
        {
            var key = PrefixKey(literatureTime.Hash);
            var task = _cacheProvider.SetAsync(key, literatureTime, TimeSpan.FromHours(2));
            tasks.Add(task);
        }

        Task.WaitAll(tasks.ToArray());

        _logger.LogInformation("Done repopulating cache");
    }

    public void Run()
    {
        intervalTimer = new Timer(
            async (timerState) =>
            {
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
                    var completeMarker = PrefixKey(COMPLETETMARKER);
                    var keyExists = await _cacheProvider.ExistsAsync(completeMarker);
                    if (keyExists)
                        return;

                    _logger.LogInformation("Marker not found");

                    await PopulateAsync();

                    await _cacheProvider.SetAsync(completeMarker, completeMarker);

                    _logger.LogInformation("Writing marker");
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
    }
}
