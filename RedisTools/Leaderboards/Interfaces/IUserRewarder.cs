using static RedisTools.Leaderboards.PeriodLeaderboardGenerator;

namespace RedisTools.Leaderboards.Interfaces;

public interface IUserRewarder
{
    Task RewardClansForBeingTopInLeaderboardAsync(List<string> sortedUserIds, string stat, LeaderBoardPeriodType periodType);
}