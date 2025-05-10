using DomainModels.Models.Game;
using DomainModels.Models.IntermediateModels;

namespace Repositories.Implementations;

public class GameHistoryRepository
{
    private readonly IDatabaseContext _databaseContext;

    public GameHistoryRepository(IDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task CreateGameHistory(MainGameHistory gameHistory)
    {
        GameHistory history = new GameHistory() 
        { 
            Claimed = gameHistory.Claimed,
            LoserName = gameHistory.LoserName,
            RewardedCoinAmount = gameHistory.RewardedCoinAmount,
            Score = gameHistory.Score,
            WinnerName = gameHistory.WinnerName
        };

        await _databaseContext.GameHistoriesCollection.InsertOneAsync(history);
    }
}
