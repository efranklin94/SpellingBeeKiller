using StackExchange.Redis;

namespace RedisTools.DistributedLock;

public class RedisLock : IDisposable
{
    private const int CHECK_INTERVAL_MILLISECONDS = 30;

    private readonly string key;
    private readonly IDatabase redis;
    private readonly DateTime ExpireDate;

    private RedisLock(IDatabase redis, string key, TimeSpan expiry)
    {
        this.redis = redis;
        this.key = key;
        this.ExpireDate = DateTime.UtcNow.Add(expiry);
    }

    static internal async Task<RedisLock> AcquireAsync(IDatabase redis, string key, TimeSpan expiry)
    {
        var _lock = new RedisLock(redis, key, expiry);

        while (true)
        {
            var tran = redis.CreateTransaction();
            tran.AddCondition(Condition.KeyNotExists(key));
            tran.StringSetAsync(key, 1, expiry);
            bool commited = await tran.ExecuteAsync();
            if (commited)
            {
                return _lock;
            }

            // check for expiry
            if (DateTime.UtcNow > _lock.ExpireDate)
            {
                throw new Exception($"Failed to aquire lock {key}, Timeout");
            }

            await Task.Delay(CHECK_INTERVAL_MILLISECONDS);
        }
    }

    static internal RedisLock Acquire(IDatabase redis, string key, TimeSpan expiry)
    {
        var _lock = new RedisLock(redis, key, expiry);

        while (true)
        {
            var tran = redis.CreateTransaction();
            tran.AddCondition(Condition.KeyNotExists(key));
            tran.StringSetAsync(key, 1, expiry);
            bool commited = tran.Execute();
            if (commited)
            {
                return _lock;
            }

            // check for expiry
            if (DateTime.UtcNow > _lock.ExpireDate)
            {
                throw new Exception($"Failed to aquire lock {key}, Timeout");
            }

            Thread.Sleep(CHECK_INTERVAL_MILLISECONDS);
            //Task.Delay(CHECK_INTERVAL_MILLISECONDS).Wait();
        }
    }

    public void Dispose()
    {
        redis.KeyDelete(key);
    }
}