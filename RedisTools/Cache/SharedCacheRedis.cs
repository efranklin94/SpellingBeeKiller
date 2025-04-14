using StackExchange.Redis;

namespace RedisTools.Cache;

public class SharedCacheRedis
{
    private readonly IDatabase db;
    private readonly char delimiterChar;

    public SharedCacheRedis(ConnectionMultiplexer connectionMultiplexer, int dbNumber = 0, char delimiterChar = '~')
    {
        this.delimiterChar = delimiterChar;
        db = connectionMultiplexer.GetDatabase(dbNumber);
    }

    public async Task<bool> HashSetAsync(RedisKey tableKey, RedisKey hashKey, RedisValue hashValue, TimeSpan? timeToLive)
    {
        return await db.StringSetAsync(ConvertHashToRedisKey(tableKey!, hashKey!), hashValue, timeToLive);
    }

    public async Task<bool> HashExistsAsync(RedisKey tableKey, RedisKey hashKey)
    {
        return await db.KeyExistsAsync(ConvertHashToRedisKey(tableKey!, hashKey!));
    }


    public async Task<RedisValue> HashGetAsync(RedisKey tableKey, RedisKey hashKey)
    {
        return await db.StringGetAsync(ConvertHashToRedisKey(tableKey!, hashKey!));
    }

    public async Task<bool> HashRemoveAsync(RedisKey tableKey, RedisKey hashKey)
    {
        return await db.KeyDeleteAsync(ConvertHashToRedisKey(tableKey!, hashKey!));
    }

    public async Task<bool> SetContainsAsync(RedisKey table, RedisKey setKey)
    {
        return await HashExistsAsync(table, setKey);
    }

    public async Task<bool> SetAddAsync(RedisKey table, RedisKey setKey, TimeSpan? timeToLive)
    {
        return await HashSetAsync(table, setKey, "", timeToLive);
    }

    public async Task<bool> SetRemoveAsync(RedisKey table, RedisKey setKey)
    {
        return await HashRemoveAsync(table, setKey);
    }

    private string ConvertHashToRedisKey(string tableKey, string hashKey)
    {
        if (tableKey.Contains(delimiterChar) || hashKey.Contains(delimiterChar))
        {
            throw new Exception("TableKey or HashKey Contains Delimiter Character!");
        }

        return string.Format("{0}{1}{2}", tableKey, delimiterChar, hashKey);
    }

    private (string tableKey, string hashKey) ConvertRedisKeyToHash(string redisKey)
    {
        var values = redisKey.Split(delimiterChar);
        if (values.Length > 2)
        {
            throw new Exception("Failed to get Hash Values from RedisKey, " +
                "rediskey contains delimiter char more than once");
        }

        return (values[0], values[1]);
    }

    public async Task<long> HashDeleteAsync(string tableKey, List<string> hashKeys)
    {
        //return await db.KeyDeleteAsync(ConvertHashToRedisKey(tableKey!, hashKey!));
        var keysToDelete = hashKeys.Select(x => (StackExchange.Redis.RedisKey)ConvertHashToRedisKey(tableKey, x)).ToArray();
        return await db.KeyDeleteAsync(keysToDelete);
    }
}