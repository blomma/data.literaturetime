namespace Data.LiteratureTime.Core.Workers.v2;

using Data.LiteratureTime.Core.Interfaces.v2;
using Data.LiteratureTime.Core.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Irrbloss;

public class LiteratureDataWorker
{
    private readonly ILiteratureService _literatureService;
    private Timer? timer;
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

    ~LiteratureDataWorker()
    {
        timer?.Dispose();

        _lockSemaphore.Dispose();
    }

    private static string PrefixKey(string key) => $"{KEY_PREFIX}:{key}";

    public void Run()
    {
        timer = new Timer(
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
                            (db) => db.KeyExistsAsync(completeMarker)
                        )
                    )
                        return;

                    _logger.LogInformation(
                        "COMPLETETMARKER:{completeMarker} not found",
                        completeMarker
                    );
                    var literatureTimes = await _literatureService.GetLiteratureTimesAsync();

                    ILookup<string, LiteratureTime> lookup = literatureTimes.ToLookup(o => o.Time);
                    foreach (IGrouping<string, LiteratureTime> literatureTimesGroup in lookup)
                    {
                        var jsonData = JsonSerializer.Serialize(literatureTimesGroup.ToList());
                        var result = await _redisConnection.BasicRetryAsync(
                            (db) => db.StringSetAsync(PrefixKey(literatureTimesGroup.Key), jsonData)
                        );
                    }

                    await _redisConnection.BasicRetryAsync(
                        (db) => db.StringSetAsync(completeMarker, completeMarker)
                    );
                    _logger.LogInformation("COMPLETETMARKER:{completeMarker} set", completeMarker);
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
            5000
        );
    }
}
