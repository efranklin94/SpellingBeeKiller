using StackExchange.Redis;
using static RedisTools.Leaderboards.SegmentedTournamentGenerator;

namespace RedisTools.Leaderboards.Interfaces;

public interface ISegmentedTournamentGenerator
{
    Task<(bool success, string error)> CreateTournamentDetailsAsync(TournamentDetails details);
    Task<TournamentDetails> GetTournamentDetailsAsync(string tournament);
    Task<bool> UpateTournamentDetailsAsync(string tournament, TournamentDetails newDetails);
    SharedLeaderboardRedis GetSharedLeaderboard();
    Task SetActiveTournamentAsync(string tournament, bool active);
    Task SetUpcommingTournamentAsync(string tournament, bool active);
    Task SetToFutureTournamentAsync(string tournament, bool active);
    Task<bool> ActiveTournamentExistsAsync(string tournament);
    Task<bool> UpcomingTournamentExistsAsync(string tournament);
    Task<bool> FutureTournamentExistsAsync(string tournament);
    Task<RedisValue[]> GetActiveTournamentsAsync();
    Task<RedisValue[]> GetUpcomingTournamentsAsync();
    Task<RedisValue[]> GetFutureTournamentsAsync();
    Task<List<TournamentDetails>> GetActiveTournamentsWithDetailsAsync();
    Task RemoveTournamentAsync(string tournament);
    Task<string> FindSegmentAndAddUserToItAsync(string userId, string tournament);
    Task RemoveUserFromSegmentAsync(string tournament, string segment, string userId);
    Task<long> GetTotalSegmentForTournamentAsync(string tournament);
    Task<List<string>> GetAllSegmentsAsync(string tournament);
    Task<double> IncrementUserScoreAsync(string userId, string tournament, double score);
    Task<double> DecrementUserScoreAsync(string userId, string tournament, double amount);
    Task<bool> SetUserScoreAsync(string userId, string tournament, double totalScore);
    Task UpdateBotsScoresAsync(string tournament);
    Task<SortedSetEntry[]> GetBestOfLeaderboardAsync(string tournament, string segment, int maxCount);
    Task<string> GetUserSegmentAsync(string userId, string tournament);
    Task<(long rank, string error)> GetUserRankAsync(string tournament, string userId, string segment = null!);
    Task<(long rank, double score, string error)> GetUserRankWithScoreAsync(string tournament, string userId, string segment = null!);
    Task<long> GetSegmentLengthAsync(string tournament, string segment);
}