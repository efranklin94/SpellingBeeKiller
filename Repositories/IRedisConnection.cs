using RedisTools.Cache;
using RedisTools.Leaderboards;
using StackExchange.Redis;

namespace Repositories
{
    public interface IRedisConnection
    {
        public IDatabase GetDlockRedisDb();
        public IDatabase GetRedisDb();
        public SharedCacheRedis GetSharedCacheRedis();
        public IDatabase GetSimpleCacheRedis();
        public IDatabase GetClanCacheRedisDb();
        public SharedLeaderboardRedis GetLeaderboardRedis();
        public IDatabase GetFriendsRedisCacheDb();
    }
}
