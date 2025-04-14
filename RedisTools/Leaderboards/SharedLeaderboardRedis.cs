using StackExchange.Redis;

namespace RedisTools.Leaderboards
{
    public class SharedLeaderboardRedis
    {
        public readonly IDatabase database;
        private readonly string LeaderboardsSet = "ACTIVE_LEADERBOARDS";

        public SharedLeaderboardRedis(IDatabase database)
        {
            this.database = database;
        }

        public async Task SaveScoreAsync(string stat, string label, string userId, double score)
        {
            await database.SortedSetAddAsync(ConvertLeaderboardItemsToRedisKey(stat, label), userId, score);
        }

        public async Task<double> IncrementScoreAsync(string stat, string label, string userId, double addedScore)
        {
            return await database.SortedSetIncrementAsync(ConvertLeaderboardItemsToRedisKey(stat, label), userId, addedScore);
        }

        public async Task<double> DecrementScoreAsync(string stat, string label, string userId, double amount)
        {
            return await database.SortedSetDecrementAsync(ConvertLeaderboardItemsToRedisKey(stat, label), userId, amount);
        }

        public async Task<bool> RemoveItemFromLeaderboard(string stat, string label, string userId)
        {
            return await database.SortedSetRemoveAsync(ConvertLeaderboardItemsToRedisKey(stat, label), userId);
        }

        public async Task<SortedSetEntry[]> GetBestOfLeaderboardAsync(string stat, string label, int maxResultsCount = 30)
        {
            var redisRespnse = await database.SortedSetRangeByRankWithScoresAsync(
                ConvertLeaderboardItemsToRedisKey(stat, label), 0, maxResultsCount - 1, StackExchange.Redis.Order.Descending
                );
            return redisRespnse;
        }

        public async Task<long?> GetItemRankZeroBased(string stat, string label, string userId)
        {
            return await database.SortedSetRankAsync(ConvertLeaderboardItemsToRedisKey(stat, label), userId, Order.Descending);
        }

        public async Task<double?> GetItemScore(string stat, string label, string userId)
        {
            return await database.SortedSetScoreAsync(ConvertLeaderboardItemsToRedisKey(stat, label), userId);
        }

        public async Task<(long? rank, double? score)> GetItemRankZeroBasedWithScore(string stat, string label, string userId)
        {
            var rank = await GetItemRankZeroBased(stat, label, userId);
            var score = await GetItemScore(stat, label, userId);
            return (rank, score);
        }

        public async Task<long> GetLength(string stat, string label)
        {
            return await database.SortedSetLengthAsync(ConvertLeaderboardItemsToRedisKey(stat, label));
        }

        public async Task<List<string>> GetItemsAroundScoreAsync(double score, string stat, string label, int maxResultsCount = 30)
        {
            var lowerPlayersResponse = await database.SortedSetRangeByScoreAsync(ConvertLeaderboardItemsToRedisKey(stat, label),
                0, score, StackExchange.Redis.Exclude.None, StackExchange.Redis.Order.Descending,
                0, maxResultsCount / 2);

            var upperPlayersResponse = await database.SortedSetRangeByScoreAsync(ConvertLeaderboardItemsToRedisKey(stat, label),
                score, double.PositiveInfinity, StackExchange.Redis.Exclude.None, StackExchange.Redis.Order.Ascending,
                0, maxResultsCount / 2);

            var userIds = lowerPlayersResponse.Select(x => x.ToString()).ToList();
            var upperPlayersIds = upperPlayersResponse.Select(x => x.ToString()).ToList();
            userIds.AddRange(upperPlayersIds);

            return userIds;
        }

        public async Task<bool> ClearLeaderboard(string stat, string label)
        {
            return await database.KeyDeleteAsync(ConvertLeaderboardItemsToRedisKey(stat, label));
        }

        public async Task<bool> AddLeaderboardAsync(string stat, string label)
        {
            return await database.SetAddAsync(LeaderboardsSet, ConvertLeaderboardItemsToRedisKey(stat, label));
        }

        public async Task<bool> LeaderboardExistsAsync(string stat, string label)
        {
            return await database.SetContainsAsync(LeaderboardsSet, ConvertLeaderboardItemsToRedisKey(stat, label));
        }

        public string ConvertLeaderboardItemsToRedisKey(string stat, string label)
        {
            return string.Format("{0}-{1}", stat, label);
        }
    }
}
