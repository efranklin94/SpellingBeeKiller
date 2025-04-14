using StackExchange.Redis;

namespace RedisTools.DistributedLock;

public static class DistributedLocExtentions
{
    public static async Task<RedisLock> AcquireLockAsync(this IDatabase database, string resource, TimeSpan expiry)
    {
        return await RedisLock.AcquireAsync(database, resource, expiry);
    }

    public static RedisLock AcquireLock(this IDatabase database, string resource, TimeSpan expiry)
    {
        return RedisLock.Acquire(database, resource, expiry);
    }
}
