using DomainModels.Models;
using DomainModels.Models.Game;
using DomainModels.Models.IntermediateModels;
using MongoDB.Driver;
using Repositories.Contracts;
using Repositories.Implementations;

namespace DomainServices.Implementations;

public class GameService
{
    private readonly CoreBeeGameRedisRepository coreBeeGameRedisRepository;
    private readonly GameHistoryRepository gameHistoryRepository;
    private readonly IUserRepository userRepository;

    public GameService(CoreBeeGameRedisRepository coreBeeGameRedisRepository, GameHistoryRepository gameHistoryRepository, IUserRepository userRepository)
    {
        this.coreBeeGameRedisRepository = coreBeeGameRedisRepository;
        this.gameHistoryRepository = gameHistoryRepository;
        this.userRepository = userRepository;
    }

    public async Task<CoreBeeGameData> CreateGameAsync(string firstUserId, string secondUserId)
    {
        // Create the game in redis
        CoreBeeGameData gameData = new CoreBeeGameData()
        {
            GameId = "game00",
            PlayerRoomHostId = firstUserId,
            PlayerRoomGuestId = secondUserId,
            RoundLogs = new List<CoreBeeGameRoundLog>(),
        };
        
        await coreBeeGameRedisRepository.AddOrUpdateAsync(firstUserId, gameData);


        // Update Users
        var updateDefinitions = new Dictionary<string, UpdateDefinition<User>>();
        
        User firstUser = await userRepository.GetUserByIdAsync(firstUserId);
        User secondUser = await userRepository.GetUserByIdAsync(secondUserId);

        firstUser.XP++; secondUser.XP++;
        firstUser.Ticket--; secondUser.Ticket--;

        updateDefinitions[firstUserId] = (Builders<User>.Update.Set(x => x.Ticket, firstUser.Ticket).Set(x => x.XP, firstUser.XP));
        updateDefinitions[secondUserId] = (Builders<User>.Update.Set(x => x.Ticket, secondUser.Ticket).Set(x => x.XP, secondUser.XP));

        await userRepository.BulkUpdateUsersAsync(updateDefinitions);


        return gameData;
    }

    public async Task FinishGameAsync(MainGameHistory gameHistory)
    {
        // Transfer the game history from redis to mongo
        await coreBeeGameRedisRepository.RemoveAsync("", gameHistory.GameId);
        await gameHistoryRepository.CreateGameHistory(gameHistory);

        // Give award the winner
        User winnerUser = await userRepository.GetUserByUsernameAsync(gameHistory.WinnerName);

        winnerUser.Coin += gameHistory.RewardedCoinAmount;

        var update = Builders<User>.Update
            .Set(x => x.UpdatedAt, DateTime.UtcNow)
            .Set(x => x.Coin, winnerUser.Coin);

        await userRepository.UpdateUserByIdAsync(winnerUser.UserId, update);
    }
}
