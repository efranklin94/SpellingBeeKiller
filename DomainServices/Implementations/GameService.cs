using DomainModels.DTO;
using DomainModels.Models;
using DomainModels.Models.Game;
using DomainModels.Models.IntermediateModels;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Repositories.Contracts;
using Repositories.Implementations;

namespace DomainServices.Implementations;

public class GameService
{
    private readonly CoreBeeGameRedisRepository coreBeeGameRedisRepository;
    private readonly GameHistoryRepository gameHistoryRepository;
    private readonly IUserRepository userRepository;
    private readonly IHubContext<GameHub> hubContext;

    public GameService(CoreBeeGameRedisRepository coreBeeGameRedisRepository, GameHistoryRepository gameHistoryRepository, IUserRepository userRepository, IHubContext<GameHub> hubContext)
    {
        this.coreBeeGameRedisRepository = coreBeeGameRedisRepository;
        this.gameHistoryRepository = gameHistoryRepository;
        this.userRepository = userRepository;
        this.hubContext = hubContext;
    }

    public async Task<(int firstUserUpdatedTicket, string gameId)> CreateGameAsync(string userId)
    {
        // Create the game in redis
        CoreBeeGameData gameData = new CoreBeeGameData()
        {
            GameId = Guid.NewGuid().ToString(),
            PlayerRoomHostId = userId,
            RoundLogs = new List<CoreBeeGameRoundLog>(),
            CreatedAt = DateTime.Now
        };
        
        await coreBeeGameRedisRepository.AddOrUpdateAsync(userId, gameData);

        // Reduce ticket from host
        User user = await userRepository.GetUserByIdAsync(userId);
        user.Ticket--;
        user.XP++;

        await userRepository.UpdateUserByIdAsync(userId,
            Builders<User>.Update
                .Set(x => x.Ticket, user.Ticket)
                .Set(x => x.UpdatedAt, DateTime.UtcNow));

        return (user.Ticket, gameData.GameId);
    }

    public async Task<(int updatedTicket, CoreBeeGameData game)> JoinGameAsync(string gameId, string userId)
    {
        var game = await coreBeeGameRedisRepository.GetAsync("", gameId);
        if (game == null) throw new Exception("Game not found");

        // Add second player
        game.PlayerRoomGuestId = userId;
        await coreBeeGameRedisRepository.AddOrUpdateAsync("", game);

        // Reduce ticket from guest
        var user = await userRepository.GetUserByIdAsync(userId);
        user.Ticket--;
        user.XP++;

        await userRepository.UpdateUserByIdAsync(userId,
            Builders<User>.Update
                .Set(x => x.Ticket, user.Ticket)
                .Set(x => x.UpdatedAt, DateTime.UtcNow));

        // Notify the host
        await hubContext.Clients.User(game.PlayerRoomHostId).SendAsync("GameStart", game);

        return (user.Ticket, game);
    }

    public async Task SaveGameProgress(MainGameLogProgressRequestModel dto)
    {
        // Update the rounds
        CoreBeeGameData coreBeeGameData = await coreBeeGameRedisRepository.GetAsync("", dto.GameId);
        coreBeeGameData.RoundLogs.Add(dto.Round);
        await coreBeeGameRedisRepository.AddOrUpdateAsync("", coreBeeGameData);

        // find the other player
        var userPlayed = coreBeeGameData.RoundLogs.Last().Username;
        string turnedUserId = userPlayed == coreBeeGameData.PlayerRoomHostId ? coreBeeGameData.PlayerRoomHostId : coreBeeGameData.PlayerRoomGuestId;
        // and send the updated data to the other player
        await hubContext.Clients.User(turnedUserId).SendAsync("GameProgress", coreBeeGameData);
    }


    public async Task FinishGameAsync(string winnerUserId, string gameId)
    {
        User winnerUser = await userRepository.GetUserByIdAsync(winnerUserId);

        CoreBeeGameData gameData = await coreBeeGameRedisRepository.GetAsync("", gameId);

        string loserId = winnerUserId == gameData.PlayerRoomHostId ? gameData.PlayerRoomHostId : gameData.PlayerRoomGuestId;
        User loserUser = await userRepository.GetUserByIdAsync(loserId);

        MainGameHistory gameHistory = new MainGameHistory();
        gameHistory.GameId = gameId;
        gameHistory.WinnerName = winnerUser.Username;
        gameHistory.LoserName = loserUser.Username;
        gameHistory.Score = 50;
        gameHistory.RewardedCoinAmount = 50;

        // Transfer the game history from redis to mongo
        await coreBeeGameRedisRepository.RemoveAsync("", gameId);
        await gameHistoryRepository.CreateGameHistory(gameHistory);

        // Give award to the winner
        winnerUser.Coin += gameHistory.RewardedCoinAmount;
        var update = Builders<User>.Update
            .Set(x => x.UpdatedAt, DateTime.UtcNow)
            .Set(x => x.Coin, winnerUser.Coin);
        await userRepository.UpdateUserByIdAsync(winnerUser.Id, update);

        // Notify both users
        await hubContext.Clients.User(winnerUser.Id).SendAsync("GameFinished", gameHistory);
        await hubContext.Clients.User(loserUser.Id).SendAsync("GameFinished", gameHistory);
    }
}


/*        UserBaseModel hostUser = new UserBaseModel();
        hostUser.UserId = firstUserId;
        hostUser.NickName = firstUser.Username;
        hostUser.Level = firstUser.Level;
        
        UserBaseModel guestUser = new UserBaseModel();
        hostUser.UserId = secondUserId;
        hostUser.NickName = secondUser.Username;
        hostUser.Level = secondUser.Level;

        CoreBeeGameDataDTO gameResponse = new CoreBeeGameDataDTO();
        gameResponse.GameId = gameData.GameId;
        gameResponse.RoundLogs = gameData.RoundLogs;
        gameResponse.TimePerTurnInHours = gameData.TimePerTurnInHours;
        gameResponse.PlayerRoomHost = hostUser;
        gameResponse.PlayerRoomGuest = guestUser;*/