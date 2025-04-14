using Microsoft.Extensions.Configuration;
using RedisTools;
using RedisTools.Cache;
using RedisTools.Leaderboards;
using StackExchange.Redis;

namespace Repositories
{
    public class RedisConnection : IRedisConnection
    {
        public readonly SharedCacheRedis CacheRedis;
        private readonly RedisConnectionHandler MainRedisConnectionHandler;
        private readonly ConnectionMultiplexer cacheRedisConnectionMultiplexer;

        public readonly SharedLeaderboardRedis LeaderboardRedis;

        public RedisConnection(IConfiguration configuration)
        {
            // Main Redis
            MainRedisConnectionHandler = new RedisConnectionHandler(configuration.GetConnectionString("RedisDbConnection")!);

            // Cache Redis
            cacheRedisConnectionMultiplexer =
                new RedisConnectionHandler(configuration.GetConnectionString("CacheRedisDbConnection")!).GetConnection();
            // TODO: change cache db number for global release(we may use 1 redis instance for both). Check Bingo
            CacheRedis = new SharedCacheRedis(cacheRedisConnectionMultiplexer, 2);

            LeaderboardRedis = new SharedLeaderboardRedis(MainRedisConnectionHandler.GetConnection().GetDatabase(5));
        }

        public IDatabase GetSimpleCacheRedis()
        {
            return cacheRedisConnectionMultiplexer.GetDatabase(4);
        }

        public SharedCacheRedis GetSharedCacheRedis()
        {
            return CacheRedis;
        }

        public IDatabase GetRedisDb()
        {
            return MainRedisConnectionHandler.GetConnection().GetDatabase(2);
        }

        public IDatabase GetDlockRedisDb()
        {
            return MainRedisConnectionHandler.GetConnection().GetDatabase(15);
        }

        public IDatabase GetClanCacheRedisDb()
        {
            return MainRedisConnectionHandler.GetConnection().GetDatabase(7);
        }

        public IDatabase GetFriendsRedisCacheDb()
        {
            return MainRedisConnectionHandler.GetConnection().GetDatabase(3);
        }

        public SharedLeaderboardRedis GetLeaderboardRedis()
        {
            return LeaderboardRedis;
        }
    }

}
