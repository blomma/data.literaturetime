namespace Data.LiteratureTime.Core.Workers.v2;

using Data.LiteratureTime.Core.Interfaces.v2;
using Data.LiteratureTime.Core.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Irrbloss;
using System.Threading.Tasks;

public class LiteratureDataWorker
{
    private readonly ILiteratureService _literatureService;
    private Timer? intervalTimer;
    private Timer? sanityCheckTimer;

    private readonly SemaphoreSlim _lockSemaphore = new(initialCount: 1, maxCount: 1);

    private const string COMPLETETMARKER = "LITERATURE_COMPLETE_MARKER";
    private const string KEY_PREFIX = "LIT_V2";

    private readonly ILogger<LiteratureDataWorker> _logger;
    private readonly RedisConnection _redisConnection;

    public LiteratureDataWorker(
        ILiteratureService literatureService,
        ILogger<LiteratureDataWorker> logger,
        RedisConnection redisConnection
    )
    {
        _literatureService = literatureService;
        _logger = logger;
        _redisConnection = redisConnection;
    }

    private static string PrefixKey(string key) => $"{KEY_PREFIX}:{key}";

    private async Task PopulateAsync()
    {
        _logger.LogInformation("Repopulating cache");

        var literatureTimes = await _literatureService.GetLiteratureTimesAsync();

        ILookup<string, LiteratureTime> lookup = literatureTimes.ToLookup(o => o.Time);
        foreach (IGrouping<string, LiteratureTime> literatureTimesGroup in lookup)
        {
            var jsonData = JsonSerializer.Serialize(literatureTimesGroup.ToList());
            _ = await _redisConnection.BasicRetryAsync(
                (db, state) =>
                {
                    var (key, data, expiry) = state;
                    return db.StringSetAsync(key, data, expiry);
                },
                (PrefixKey(literatureTimesGroup.Key), jsonData, TimeSpan.FromHours(2))
            );
        }

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
                    if (
                        await _redisConnection.BasicRetryAsync(
                            (db, marker) =>
                            {
                                return db.KeyExistsAsync(marker);
                            },
                            completeMarker
                        )
                    )
                        return;

                    _logger.LogInformation("Marker not found");

                    await PopulateAsync();

                    await _redisConnection.BasicRetryAsync(
                        (db, marker) =>
                        {
                            return db.StringSetAsync(marker, marker);
                        },
                        completeMarker
                    );

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
