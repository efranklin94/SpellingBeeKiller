using RedisTools.Cache;
using RedisTools.Leaderboards.Interfaces;
using StackExchange.Redis;

namespace RedisTools.Leaderboards;

public class PeriodLeaderboardGenerator
{
    private readonly SharedLeaderboardRedis database;
    private readonly SharedCacheRedis CacheDb;
    private readonly ILeaderboardJobScheduler jobScheduler;
    private readonly IUserRewarder userRewarder;

    private const string CacheTableName = "scoreTransactinNounce";

    public PeriodLeaderboardGenerator(SharedLeaderboardRedis leaderboardDb, SharedCacheRedis cacheDb,
        ILeaderboardJobScheduler jobScheduler, IUserRewarder userRewarder)
    {
        this.jobScheduler = jobScheduler;
        this.userRewarder = userRewarder;
        this.database = leaderboardDb;
        this.CacheDb = cacheDb;
    }

    public async Task AddLeaderboardAsync(string stat, LeaderBoardPeriodType type)
    {
        var result = await database.AddLeaderboardAsync(stat, type.ToString());
        // register job scheduler for reseting
        jobScheduler.ScheduleRecurringJob(stat, type);

    }

    public async Task IncScoreAsync(string stat, string userId, double addedScore, string transactionNounce,
        List<LeaderBoardPeriodType> leaderBoardPeriodTypes)
    {
        // check if score added before or not
        if (await CacheDb.SetContainsAsync(CacheTableName, $"{userId}-{transactionNounce}"))
        {
            return;
        }

        foreach (var boardPeriodType in leaderBoardPeriodTypes)
        {
            // create leaderboard if not available
            if (!await LeaderboardExistsAsync(stat, boardPeriodType))
            {
                await AddLeaderboardAsync(stat, boardPeriodType);
            }
            // add score
            await database.IncrementScoreAsync(stat, boardPeriodType.ToString(), userId, addedScore);
        }

        // save it to cache table to be able to block repeatative save score requests
        await CacheDb.SetAddAsync(CacheTableName, $"{userId}-{transactionNounce}", TimeSpan.FromDays(1));
    }

    #region Jobs

    public async Task<bool> ClearWeeklyLeaderboard(string stat)
    {
        // get top players
        var redisValues = await database.GetBestOfLeaderboardAsync(stat, LeaderBoardPeriodType.Weekly.ToString());

        var topPlayers = redisValues.Select(x => x.Element.ToString()).ToList();
        // call the rewarder
        await userRewarder.RewardClansForBeingTopInLeaderboardAsync(topPlayers, stat, LeaderBoardPeriodType.Weekly);
        // clear the leaderboard
        return await database.ClearLeaderboard(stat, LeaderBoardPeriodType.Weekly.ToString());
    }

    public async Task<bool> ClearMonthlyLeaderboard(string stat)
    {
        // get top players
        var redisValues = await database.GetBestOfLeaderboardAsync(stat, LeaderBoardPeriodType.Monthly.ToString());

        var topPlayers = redisValues.Select(x => x.Element.ToString()).ToList();
        // call the rewarder
        await userRewarder.RewardClansForBeingTopInLeaderboardAsync(topPlayers, stat, LeaderBoardPeriodType.Monthly);
        // clear the leaderboard
        return await database.ClearLeaderboard(stat, LeaderBoardPeriodType.Monthly.ToString());
    }

    #endregion

    public async Task RemoveItemFromLeaderboard(string stat, string id)
    {
        if (await LeaderboardExistsAsync(stat, LeaderBoardPeriodType.FromBegining))
        {
            // add score to "FromBegining"
            await database.RemoveItemFromLeaderboard(stat, LeaderBoardPeriodType.FromBegining.ToString(), id);
        }
        if (await LeaderboardExistsAsync(stat, LeaderBoardPeriodType.Weekly))
        {
            // add weekly score
            await database.RemoveItemFromLeaderboard(stat, LeaderBoardPeriodType.Weekly.ToString(), id);
        }
        if (await LeaderboardExistsAsync(stat, LeaderBoardPeriodType.Monthly))
        {
            // add monthly score
            await database.RemoveItemFromLeaderboard(stat, LeaderBoardPeriodType.Monthly.ToString(), id);
        }
    }

    public async Task<bool> LeaderboardExistsAsync(string stat, LeaderBoardPeriodType boardPeriodType)
    {
        // check if we have such leaderboard or not
        return await database.LeaderboardExistsAsync(stat, boardPeriodType.ToString());
    }

    public async Task<SortedSetEntry[]> GetBestItemsAsync(string stat, LeaderBoardPeriodType type, int maxResultsCount = 30)
    {
        return await database.GetBestOfLeaderboardAsync(stat, type.ToString(), maxResultsCount);
    }

    public async Task<List<string>> GetItemsAroundScoreAsync(double score, string stat, LeaderBoardPeriodType type, int maxResultsCount = 30)
    {
        return await database.GetItemsAroundScoreAsync(score, stat, type.ToString(), maxResultsCount);
    }

    public async Task<double> GetItemScore(string stat, LeaderBoardPeriodType type, string userId)
    {
        return await database.GetItemScore(stat, type.ToString(), userId) ?? 0;
    }

    public async Task<long?> GetItemRankZeroBased(string stat, LeaderBoardPeriodType type, string userId)
    {
        if (!await LeaderboardExistsAsync(stat, type))
        {
            return null;
        }
        return await database.GetItemRankZeroBased(stat, type.ToString(), userId);
    }

    public enum LeaderBoardPeriodType
    {
        Daily,
        Weekly,
        Monthly,
        Yearly,
        FromBegining
    }
}