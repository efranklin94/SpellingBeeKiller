using static RedisTools.Leaderboards.PeriodLeaderboardGenerator;

namespace RedisTools.Leaderboards.Interfaces;

public interface ILeaderboardJobScheduler
{
    void ScheduleRecurringJob(string stat, LeaderBoardPeriodType cronType);
}