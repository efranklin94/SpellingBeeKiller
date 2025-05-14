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

    public async Task<(int firstUserUpdatedTicket, CoreBeeGameData gameDataDTO)> CreateGameAsync(string userId)
    {
        // Create the game in redis
        CoreBeeGameDataDb gameData = new CoreBeeGameDataDb()
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

        CoreBeeGameData gameDataDTO = new CoreBeeGameData()
        {
            GameId = gameData.GameId,
            PlayerRoomHost = new UserBaseModel { Level = user.Level, UserId = user.Id, NickName = user.Username},
            RoundLogs = gameData.RoundLogs,
        };

        return (user.Ticket, gameDataDTO);
    }

    public async Task<(int updatedTicket, CoreBeeGameData gameData)> JoinGameAsync(string gameId, string userId)
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

        User hostUser = await userRepository.GetUserByIdAsync(game.PlayerRoomHostId);
        CoreBeeGameData gameDataDTO = new CoreBeeGameData()
        {
            GameId = game.GameId,
            PlayerRoomGuest = new UserBaseModel { UserId = user.Id, Level = user.Level, NickName = user.Username },
            RoundLogs = game.RoundLogs,
            PlayerRoomHost = new UserBaseModel { UserId = hostUser.Id, Level = hostUser.Level, NickName = hostUser.Username }
        };

        // Notify the host
        await Task.Delay(TimeSpan.FromSeconds(5));
        if (GameHub.TryGetConnectionId(gameDataDTO.PlayerRoomHost.UserId, out var connectionId))
        {
            await hubContext.Clients.User(gameDataDTO.PlayerRoomHost.UserId).SendAsync("JoinGame", gameDataDTO);
        }
        else
        {
            // Handle offline user (store notification, etc.)
        }

        return (user.Ticket, gameDataDTO);
    }

    public async Task SaveGameProgress(MainGameLogProgressRequestModel dto)
    {
        // Update the rounds
        CoreBeeGameDataDb coreBeeGameData = await coreBeeGameRedisRepository.GetAsync("", dto.GameId);
        coreBeeGameData.RoundLogs.Add(dto.Round);
        await coreBeeGameRedisRepository.AddOrUpdateAsync("", coreBeeGameData);

        // find the other player
        var usernamePlayed = coreBeeGameData.RoundLogs.Last().Username;
        User userPlayed = await userRepository.GetUserByUsernameAsync(usernamePlayed);
        string turnedPlayerId = "";
        if (userPlayed.Id == coreBeeGameData.PlayerRoomHostId)
        {
            turnedPlayerId = coreBeeGameData.PlayerRoomGuestId;
        }
        else
        {
            turnedPlayerId = coreBeeGameData.PlayerRoomHostId;
        }
        // and if the other player has joined the game, send the updated data to the other player
        if (turnedPlayerId != "")
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            if (GameHub.TryGetConnectionId(turnedPlayerId, out var connectionId))
            {
                await hubContext.Clients.Client(connectionId).SendAsync("GameProgress", coreBeeGameData);
            }
            else
            {
                // Handle offline user (store notification, etc.)
            }
        }
    }


    public async Task FinishGameAsync(string winnerUserId, string gameId)
    {
        User winnerUser = await userRepository.GetUserByIdAsync(winnerUserId);

        CoreBeeGameDataDb gameData = await coreBeeGameRedisRepository.GetAsync("", gameId);

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
        await Task.Delay(TimeSpan.FromSeconds(5));
        if (GameHub.TryGetConnectionId(winnerUser.Id, out var connectionId1))
        {
            await hubContext.Clients.User(winnerUser.Id).SendAsync("GameFinished", gameHistory);
        }
        else
        {
            // Handle offline user (store notification, etc.)
        }

        if (GameHub.TryGetConnectionId(loserUser.Id, out var connectionId2))
        {
            await hubContext.Clients.User(loserUser.Id).SendAsync("GameFinished", gameHistory);
        }
        else
        {
            // Handle offline user (store notification, etc.)
        }


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