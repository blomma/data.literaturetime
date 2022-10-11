namespace Data.LiteratureTime.Core;

using System;
using System.Threading.Tasks;
using StackExchange.Redis;

public class RedisDistributedLock
{
    public RedisKey Key { get; private set; }

    public RedisValue Value { get; private set; }

    public TimeSpan Validity { get; private set; }

    public RedisDistributedLock(RedisKey key, RedisValue value, TimeSpan validity)
    {
        Key = key;
        Value = value;
        Validity = validity;
    }
}

public class RedisDistributedLockManager
{
    private RedisDistributedLock? _redisDistributedLock;
    private readonly RedisConnection _redisConnection;

    public RedisDistributedLockManager(RedisConnection redisConnection)
    {
        _redisConnection = redisConnection;
    }

    private const string UnlockScript =
        @"
            if redis.call(""get"",KEYS[1]) == ARGV[1] then
                return redis.call(""del"",KEYS[1])
            else
                return 0
            end";

    private static byte[] CreateUniqueLockId()
    {
        return Guid.NewGuid().ToByteArray();
    }

    public async Task<bool> LockAsync(string key, TimeSpan ttl)
    {
        var value = CreateUniqueLockId();

        var result = await _redisConnection.BasicRetryAsync(
            (db) => db.StringSetAsync(key, value, ttl, When.NotExists)
        );

        if (!result)
        {
            return false;
        }

        _redisDistributedLock = new RedisDistributedLock(key, value, ttl);

        return true;
    }

    public Task UnlockAsync()
    {
        if (_redisDistributedLock == null)
        {
            return Task.CompletedTask;
        }

        RedisKey[] key = { _redisDistributedLock.Key };
        RedisValue[] values = { _redisDistributedLock.Value };

        return _redisConnection.BasicRetryAsync(
            (db) => db.ScriptEvaluateAsync(UnlockScript, key, values)
        );
    }
}