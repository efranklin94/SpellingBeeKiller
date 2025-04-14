using RedisTools.DistributedLock;
using RedisTools.Interfaces;
using RedisTools.Leaderboards.Interfaces;
using StackExchange.Redis;

namespace RedisTools.Leaderboards;

public class SegmentedTournamentGenerator : ISegmentedTournamentGenerator
{
    private readonly string activeTournamentsSet;
    private readonly string upcomingTournamentsSet;
    private readonly string futureTournamentsSet;
    private readonly SharedLeaderboardRedis leaderboards;
    private readonly IDatabase database;
    private readonly IDatabase distributedLockDb;
    private readonly Random random;
    private readonly IBotRepository botRepository;


    public SegmentedTournamentGenerator(SharedLeaderboardRedis sharedLeaderboardRedis, IDatabase distributedLockDb,
        string activeTournamentsSet, string upcomingTournamentsSet, string futureTournamentsSet, IBotRepository botManager)
    {
        this.leaderboards = sharedLeaderboardRedis;
        this.distributedLockDb = distributedLockDb;
        this.activeTournamentsSet = activeTournamentsSet;
        this.upcomingTournamentsSet = upcomingTournamentsSet;
        this.futureTournamentsSet = futureTournamentsSet;
        database = sharedLeaderboardRedis.database;
        random = new Random();
        this.botRepository = botManager;
    }

    public async Task<(bool success, string error)> CreateTournamentDetailsAsync(TournamentDetails details)
    {
        if (await ActiveTournamentExistsAsync(details.Name!))
        {
            return (false, $"Another Tournament with exact name is already active! {details.Name}");
        }
        if (details.AnounceAt > details.StartTime)
        {
            return (false, "Anounce time can not be after start time");
        }
        if (details.EndTime < details.StartTime)
        {
            return (false, "end time should be after start time");
        }
        if (details.EndTime < DateTime.UtcNow)
        {
            return (false, "end time should be after DateTime.now");
        }

        //save details to redis
        var tounamentDetails = ConvertTournamentDetailsToHashEntry(details);
        await database.HashSetAsync(GetTounamentDetailsKey(details.Name!), tounamentDetails);

        if (DateTime.UtcNow < details.AnounceAt)
        {
            await SetToFutureTournamentAsync(details.Name!, true);
        }
        else if (details.AnounceAt <= DateTime.UtcNow && DateTime.UtcNow < details.StartTime)
        {
            await SetUpcommingTournamentAsync(details.Name!, true);
        }
        else if (DateTime.UtcNow >= details.StartTime && DateTime.UtcNow < details.EndTime)
        {
            await SetActiveTournamentAsync(details.Name!, true);
        }

        return (true, null!);
    }

    public async Task SetActiveTournamentAsync(string tournament, bool active)
    {
        if (active)
        {
            await database.SetAddAsync(activeTournamentsSet, tournament);
        }
        else
        {
            await database.SetRemoveAsync(activeTournamentsSet, tournament);
        }
    }

    public async Task SetUpcommingTournamentAsync(string tournament, bool active)
    {
        if (active)
        {
            await database.SetAddAsync(upcomingTournamentsSet, tournament);
        }
        else
        {
            await database.SetRemoveAsync(upcomingTournamentsSet, tournament);
        }
    }

    public async Task SetToFutureTournamentAsync(string tournament, bool active)
    {
        if (active)
        {
            await database.SetAddAsync(futureTournamentsSet, tournament);
        }
        else
        {
            await database.SetRemoveAsync(futureTournamentsSet, tournament);
        }
    }

    public async Task<bool> ActiveTournamentExistsAsync(string tournament)
    {
        return await database.SetContainsAsync(activeTournamentsSet, tournament);
    }

    public async Task<bool> UpcomingTournamentExistsAsync(string tournament)
    {
        return await database.SetContainsAsync(upcomingTournamentsSet, tournament);
    }

    public async Task<bool> FutureTournamentExistsAsync(string tournament)
    {
        return await database.SetContainsAsync(futureTournamentsSet, tournament);
    }

    public async Task<RedisValue[]> GetActiveTournamentsAsync()
    {
        return await database.SetMembersAsync(activeTournamentsSet);
    }

    public async Task<RedisValue[]> GetUpcomingTournamentsAsync()
    {
        return await database.SetMembersAsync(upcomingTournamentsSet);
    }

    public async Task<RedisValue[]> GetFutureTournamentsAsync()
    {
        return await database.SetMembersAsync(futureTournamentsSet);
    }

    public async Task<TournamentDetails> GetTournamentDetailsAsync(string tournament)
    {
        var redisKey = GetTounamentDetailsKey(tournament);
        if (!await database.KeyExistsAsync(redisKey))
        {
            return null!;
        }

        var values = await database.HashGetAllAsync(redisKey);
        return ConvertHashEntriesToTournamentDetails(values);
    }

    public async Task<bool> UpateTournamentDetailsAsync(string tournament, TournamentDetails newDetails)
    {
        var redisKey = GetTounamentDetailsKey(tournament);
        if (!await database.KeyExistsAsync(redisKey))
        {
            throw new Exception($"Tournament Details not found for updating! {tournament}");
        }

        newDetails.Name = tournament;
        var tounamentDetails = ConvertTournamentDetailsToHashEntry(newDetails);
        await database.KeyDeleteAsync(GetTounamentDetailsKey(tournament));
        await database.HashSetAsync(GetTounamentDetailsKey(tournament), tounamentDetails);

        return true;
    }

    public async Task RemoveTournamentAsync(string tournament)
    {
        //remove tounament from active tounaments
        await SetActiveTournamentAsync(tournament, false);
        await SetUpcommingTournamentAsync(tournament, false);
        await SetToFutureTournamentAsync(tournament, false);

        // remove tounament details
        await database.KeyDeleteAsync(GetTounamentDetailsKey(tournament));

        var segments = await GetAllSegmentsAsync(tournament);
        foreach (var item in segments)
        {
            // remove segment
            await leaderboards.ClearLeaderboard(tournament, item);
        }

        // remove segments sorted set
        await database.KeyDeleteAsync(GetFillingSegmentsSortedSetKey(tournament));
        await database.KeyDeleteAsync(GetSegmentsSetKey(tournament));
        // remove user to segment hash
        await database.KeyDeleteAsync(GetUserToSegmentHashKey(tournament));
        //remove active bots
        await database.KeyDeleteAsync(GetTournamentActiveBotsKey(tournament));
    }

    public async Task<string> FindSegmentAndAddUserToItAsync(string userId, string tournament)
    {
        var tournamentDetails = await GetTournamentDetailsAsync(tournament);

        // check if we have a filling segment
        string segment = null!;

        using (await distributedLockDb.AcquireLockAsync($"AddUser-{tournament}", TimeSpan.FromSeconds(30)))
        {
            segment = await FindFillingSegment(tournament);
            if (segment != null)
            {
                //add user to it
                await AddUserToSegment(tournament, segment, userId, tournamentDetails.Capacity);
                return segment;
            }
            else
            {
                // if not create a new segment
                segment = await CreateSegment(tournament, tournamentDetails.Capacity);
                // save user-label to know which user belongs to which segment
                await AddUserToSegment(tournament, segment, userId, tournamentDetails.Capacity);
                return segment;
            }
        }
    }

    private async Task AddUserToSegment(string tournament, string segment, string userId, int segmentCapacity)
    {
        //check if we need to remove a bot
        if (await database.SortedSetLengthAsync(leaderboards.ConvertLeaderboardItemsToRedisKey(tournament, segment)) >= segmentCapacity)
        {
            //pop a bot from active bots
            var activeBot = await database.SetPopAsync(GetTournamentActiveBotsKey(tournament));
            if (activeBot != RedisValue.Null)
            {
                // remove bot from segment
                await RemoveUserFromSegmentAsync(tournament, segment, activeBot!);
            }
        }

        // add user to segment
        await database.HashSetAsync(GetUserToSegmentHashKey(tournament), userId, segment);
    }

    public async Task RemoveUserFromSegmentAsync(string tournament, string segment, string userId)
    {
        // remove from user to segment hash
        await database.HashDeleteAsync(GetUserToSegmentHashKey(tournament), userId);
        // remove from leaderboard
        await leaderboards.RemoveItemFromLeaderboard(tournament, segment, userId);
    }

    private async Task<string> FindFillingSegment(string tournament)
    {
        var segments = await database.SortedSetRangeByRankWithScoresAsync(GetFillingSegmentsSortedSetKey(tournament), 0, 1);
        if (segments == null)
        {
            return null!;
        }
        if (segments.Length == 0)
        {
            return null!;
        }
        if (segments[0].Score > 0)
        {
            database.SortedSetAdd(GetFillingSegmentsSortedSetKey(tournament), segments[0].Element, segments[0].Score - 1);
            return (segments[0].Element)!;
        }
        else
        {
            // we should remove this game as its already full
            await database.SortedSetRemoveAsync(GetFillingSegmentsSortedSetKey(tournament), segments[0].Element);
            return null!;
        }
    }

    private async Task<string> CreateSegment(string tournament, int segmentCapacity)
    {
        // get count of current segments
        var count = await database.SetLengthAsync(GetSegmentsSetKey(tournament));
        //create filling segment
        var newSegmentName = $"Segment-{count + 1}";
        await database.SortedSetAddAsync(GetFillingSegmentsSortedSetKey(tournament), newSegmentName, segmentCapacity - 1);
        //create segment
        await database.SetAddAsync(GetSegmentsSetKey(tournament), newSegmentName);

        // get tournament details
        var details = await GetTournamentDetailsAsync(tournament);

        float botPercent = 70;
        if (!string.IsNullOrWhiteSpace(details.botPercent))
        {
            botPercent = float.Parse(details.botPercent);
        }

        //get available bots
        var availableBots = await botRepository.GetAllBotsIdsAsync();

        var activeBots = new List<RedisValue>();

        // add a bunch of bots to segment
        for (int i = 0; i < Math.Min(availableBots.Count, (int)(segmentCapacity * botPercent / 100)); i++)
        {
            await AddUserToSegment(tournament, newSegmentName, availableBots[i], segmentCapacity);
            await IncrementUserScoreAsync(availableBots[i], tournament, random.Next(1, 8));
            activeBots.Add(availableBots[i]);
        }

        if (activeBots.Count > 0)
        {
            //fill active bots for this tournament
            await database.SetAddAsync(GetTournamentActiveBotsKey(tournament), activeBots.ToArray());
        }

        return newSegmentName;
    }

    public async Task<long> GetTotalSegmentForTournamentAsync(string tournament)
    {
        return await database.SetLengthAsync(GetSegmentsSetKey(tournament));
    }

    public async Task<List<string>> GetAllSegmentsAsync(string tournament)
    {
        var allSegments = new List<string>();

        var totalSegmentsCount = await GetTotalSegmentForTournamentAsync(tournament);

        for (int i = 0; i < totalSegmentsCount; i++)
        {
            allSegments.Add($"Segment-{i + 1}");
        }

        return allSegments;
    }

    public async Task<double> IncrementUserScoreAsync(string userId, string tournament, double score)
    {
        if (score < 0)
        {
            return 0;
        }

        // check if tournament is active or not
        if (!await ActiveTournamentExistsAsync(tournament))
        {
            throw new Exception("Can not set score for inactive tournaments!");
        }

        // check if user is segmented or not
        string userSegment = await GetUserSegmentAsync(userId, tournament);
        if (!string.IsNullOrEmpty(userSegment))
        {
            //update user score
            return await leaderboards.IncrementScoreAsync(tournament, userSegment, userId, score);
        }
        else
        {
            // add user to one segment
            userSegment = await FindSegmentAndAddUserToItAsync(userId, tournament);
            // save its score
            await leaderboards.SaveScoreAsync(tournament, userSegment, userId, score);

            return score;
        }
    }

    public async Task<double> DecrementUserScoreAsync(string userId, string tournament, double amount)
    {
        if (amount < 0)
        {
            return 0;
        }

        // check if tournament is active or not
        if (!await ActiveTournamentExistsAsync(tournament))
        {
            throw new Exception("Can not set score for inactive tournaments!");
        }

        // check if user is segmented or not
        string userSegment = await GetUserSegmentAsync(userId, tournament);
        if (!string.IsNullOrEmpty(userSegment))
        {
            //update user score
            return await leaderboards.DecrementScoreAsync(tournament, userSegment, userId, amount);
        }
        else
        {
            throw new Exception("this user does not joined the tournament yet!");
        }
    }

    public async Task<bool> SetUserScoreAsync(string userId, string tournament, double totalScore)
    {
        if (totalScore < 0)
        {
            return false;
        }

        // check if tournament is active or not
        if (!await ActiveTournamentExistsAsync(tournament))
        {
            throw new Exception("Can not set score for inactive tournaments!");
        }

        // check if user is segmented or not
        string userSegment = await GetUserSegmentAsync(userId, tournament);
        if (!string.IsNullOrEmpty(userSegment))
        {
            //update user score
            await leaderboards.SaveScoreAsync(tournament, userSegment, userId, totalScore);
            return true;
        }
        else
        {
            // add user to one segment
            userSegment = await FindSegmentAndAddUserToItAsync(userId, tournament);
            // save its score
            await leaderboards.SaveScoreAsync(tournament, userSegment, userId, totalScore);
            return true;
        }
    }

    /// <summary>
    /// UpdateBotsScores
    /// </summary>
    /// <param name="tournament"></param>
    /// <param name="botPassHumanChance">this parameter lets bots to get higher values than actual players, it's some value to
    /// set the growth of scores of the bots </param>
    /// <returns></returns>
    public async Task UpdateBotsScoresAsync(string tournament)
    {
        if (!database.KeyExists(GetTournamentActiveBotsKey(tournament)))
        {
            // we dont have any active bot
            return;
        }

        var activeBots = (await database.SetMembersAsync(GetTournamentActiveBotsKey(tournament))).ToList();
        if (activeBots.Count == 0)
        {
            return;
        }
        // get tournament details
        var details = await GetTournamentDetailsAsync(tournament);
        if (!details.IsActive)
        {
            return;
        }

        //find segment of bots
        var segment = await GetUserSegmentAsync(activeBots[0]!, tournament);

        // get all tournament members
        var allMembers = await leaderboards.GetBestOfLeaderboardAsync(tournament, segment, details.Capacity);

        // find mean

        // TODO
        //var mean = allMembers.Select(x => x.Score).Mean();

        // double stdDev = mean / 2;
        // create a normal distribution
        // Normal normalDist = new Normal(mean, stdDev);

        //find best score of this segment which is not a bot
        int bestHumanScore = 0;
        for (int i = 0; i < allMembers.Length; i++)
        {
            if (!activeBots.Contains(allMembers[i].Element))
            {
                bestHumanScore = (int)allMembers[i].Score;
                break;
            }
        }

        foreach (var bot in activeBots)
        {
            //find current score
            var currentScore = await leaderboards.GetItemScore(tournament, segment, bot!);

            int addedScore = (int)currentScore.Value;// TODO: do this => (int)(normalDist.Sample() - currentScore.Value);

            //do not add more than 4 score in each interval
            addedScore = Math.Min(addedScore, 4);

            // reduce bots scores to best human score, only in 5% of cases a bot can gain more score than a human
            if (random.Next(1, 20) != 1)
            {
                if (currentScore + addedScore > bestHumanScore)
                {
                    addedScore = Math.Min(bestHumanScore - (int)currentScore.Value, addedScore);
                }
            }

            if (addedScore > 0)
            {
                await IncrementUserScoreAsync(bot!, tournament, addedScore);
            }
        }
    }

    public async Task<SortedSetEntry[]> GetBestOfLeaderboardAsync(string tournament, string segment, int maxCount)
    {
        return await leaderboards.GetBestOfLeaderboardAsync(tournament, segment, maxCount);
    }

    public async Task<string> GetUserSegmentAsync(string userId, string tournament)
    {
        // find user segment
        RedisValue userSegment = await database.HashGetAsync(GetUserToSegmentHashKey(tournament), userId);
        if (userSegment.HasValue)
        {
            return userSegment!;
        }
        // not found
        return null!;
    }

    public async Task<(long rank, string error)> GetUserRankAsync(string tournament, string userId, string segment = null!)
    {
        if (segment == null)
        {
            segment = await GetUserSegmentAsync(userId, tournament);
            if (segment == null)
            {
                return (-1, "User does not participated in this tournament!");
            }
        }

        var rank = await leaderboards.GetItemRankZeroBased(tournament, segment, userId);
        if (rank.HasValue)
        {
            return (rank.Value + 1, null!);
        }
        else
        {
            return (-1, "User does not have a Rank!");
        }
    }

    public async Task<(long rank, double score, string error)> GetUserRankWithScoreAsync(string tournament, string userId, string segment = null!)
    {
        if (segment == null)
        {
            segment = await GetUserSegmentAsync(userId, tournament);
            if (segment == null)
            {
                return (-1, -1, "User did not participated in this tournament!");
            }
        }

        var result = await leaderboards.GetItemRankZeroBasedWithScore(tournament, segment, userId);
        if (result.rank.HasValue)
        {
            return (result.rank.Value + 1, result.score!.Value, null!);
        }
        else
        {
            return (-1, -1, "User does not have a Rank!");
        }
    }

    public async Task<long> GetSegmentLengthAsync(string tournament, string segment)
    {
        return await leaderboards.GetLength(tournament, segment);
    }

    #region HelperMethods

    private string GetUserToSegmentHashKey(string tournament) => $"{tournament}-UserToSegment";

    private string GetFillingSegmentsSortedSetKey(string tournament) => $"{tournament}-FillingSegments";

    private string GetSegmentsSetKey(string tournament) => $"{tournament}-AllSegments";

    private string GetTounamentDetailsKey(string tournament) => $"{tournament}-Details";

    private string GetTournamentActiveBotsKey(string tournament) => $"{tournament}-ActiveBots";

    private HashEntry[] ConvertTournamentDetailsToHashEntry(TournamentDetails details)
    {
        if (details.Options == null)
        {
            details.Options = new Dictionary<string, string>();
        }

        var fields = details.GetType().GetFields();

        foreach (var field in fields)
        {
            // we only need 4 types of data : string, int, bool, datetime

            if (field.FieldType == typeof(string))
            {
                if (field.GetValue(details) != null)
                {
                    details.Options[field.Name] = field.GetValue(details)!.ToString()!;
                }
            }
            else if (field.FieldType == typeof(int))
            {
                if (field.GetValue(details) != null)
                {
                    details.Options[field.Name] = field.GetValue(details)!.ToString()!;
                }
            }
            else if (field.FieldType == typeof(bool))
            {
                if (field.GetValue(details) != null)
                {
                    details.Options[field.Name] = field.GetValue(details)!.ToString()!;
                }
            }
            else if (field.FieldType == typeof(DateTime))
            {
                if (field.GetValue(details) != null)
                {
                    details.Options[field.Name] = DateTime.Parse(field.GetValue(details)!.ToString()!).ToString();
                }
            }
        }

        return details.Options.Select(
            pair => new HashEntry(pair.Key, pair.Value)).ToArray();
    }

    private TournamentDetails ConvertHashEntriesToTournamentDetails(HashEntry[] hashEntries)
    {
        var options = hashEntries.ToStringDictionary();
        var details = new TournamentDetails();
        var fields = details.GetType().GetFields();

        foreach (var field in fields)
        {
            if (options.ContainsKey(field.Name))
            {
                // we only need 4 types of data : string, int, bool, datetime
                if (field.FieldType == typeof(string))
                {
                    field.SetValue(details, options[field.Name]);
                }
                else if (field.FieldType == typeof(int))
                {
                    field.SetValue(details, int.Parse(options[field.Name]));
                }
                else if (field.FieldType == typeof(bool))
                {
                    field.SetValue(details, bool.Parse(options[field.Name]));
                }
                else if (field.FieldType == typeof(DateTime))
                {
                    field.SetValue(details, DateTime.Parse(options[field.Name]));
                }
            }
        }

        details.Options = options;

        return details;
    }

    public SharedLeaderboardRedis GetSharedLeaderboard()
    {
        return leaderboards;
    }

    public async Task<List<TournamentDetails>> GetActiveTournamentsWithDetailsAsync()
    {
        var activeTournaments = await GetActiveTournamentsAsync();
        var activeEventModels = new List<TournamentDetails>();

        foreach (var item in activeTournaments)
        {
            var details = await GetTournamentDetailsAsync(item);
            if (details != null)
            {
                activeEventModels.Add(details);
            }
        }

        return activeEventModels;
    }

    #endregion

    public class TournamentDetails
    {
        /// <summary>
        /// do not change these fields to property, cause we are using reflection to convert these field to redis keys
        /// only 4 types of data get converted : string, bool, int, datetime
        /// </summary>

        public int Capacity;
        public string? Name;
        public DateTime AnounceAt;
        public DateTime StartTime;
        public DateTime EndTime;
        public Dictionary<string, string>? Options;
        public string? Type;
        public string? Description;
        public bool Recurring;
        public int MinLevelLimit;
        public int MaxLeveLLimit;
        public string? PrizeDetailsBlob;
        public string? eventType;
        public string? botPercent;

        public bool IsActive
        {
            get
            {
                if (DateTime.UtcNow < EndTime && DateTime.UtcNow > StartTime)
                {
                    return true;
                }
                return false;
            }
        }
    }
}