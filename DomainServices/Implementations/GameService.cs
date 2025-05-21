using DomainModels.DTO;
using DomainModels.Models;
using DomainModels.Models.Game;
using DomainModels.Models.IntermediateModels;
using Hangfire;
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
    private readonly IBackgroundJobClient backgroundJobClient;

    public GameService(CoreBeeGameRedisRepository coreBeeGameRedisRepository, GameHistoryRepository gameHistoryRepository, IUserRepository userRepository, IHubContext<GameHub> hubContext, IBackgroundJobClient backgroundJobClient)
    {
        this.coreBeeGameRedisRepository = coreBeeGameRedisRepository;
        this.gameHistoryRepository = gameHistoryRepository;
        this.userRepository = userRepository;
        this.hubContext = hubContext;
        this.backgroundJobClient = backgroundJobClient;
    }

    public async Task<(int firstUserUpdatedTicket, CoreBeeGameData gameDataDTO)> CreateGameAsync(string userId)
    {
        User user = await userRepository.GetUserByIdAsync(userId);

        // Create the game in redis
        CoreBeeGameData gameData = new CoreBeeGameData()
        {
            GameId = Guid.NewGuid().ToString(),
            PlayerRoomHost = new UserBaseModel
            { 
                Level = user.Level,
                NickName = user.Username,
                UserId = user.Id
            },
            RoundLogs = new List<CoreBeeGameRoundLog>(),
            CreatedAt = DateTime.Now
        };
        
        await coreBeeGameRedisRepository.AddOrUpdateAsync(userId, gameData);

        // Reduce ticket from host
        user.Ticket--;
        user.XP++;

        await userRepository.UpdateUserByIdAsync(userId,
            Builders<User>.Update
                .Set(x => x.Ticket, user.Ticket)
                .Set(x => x.UpdatedAt, DateTime.UtcNow));

        // Schedule FinishGameAsync to run in 2 hours
        // TODO calculate the winner`s score to decide which userId shall be passed
        backgroundJobClient.Schedule<GameService>(
            service => service.FinishGameAsync(userId, gameData.GameId),
            TimeSpan.FromSeconds(20)
        );

        return (user.Ticket, gameData);
    }

    public async Task<(int updatedTicket, CoreBeeGameData gameData)> JoinGameAsync(string gameId, string userId)
    {
        var gameData = await coreBeeGameRedisRepository.GetAsync("", gameId);
        if (gameData == null) throw new Exception("Game not found");

        // Check if the user was joined to the room or not 
        var user = await userRepository.GetUserByIdAsync(userId);
        if (user.Id == gameData.PlayerRoomHost!.UserId || user.Id == gameData.PlayerRoomGuest?.UserId)
        {
            throw new Exception("You have already joined the game");
        }    
        
        // Add second player
        gameData.PlayerRoomGuest = new UserBaseModel { 
            UserId = userId,
            Level = user.Level,
            NickName = user.Username
        };

        await coreBeeGameRedisRepository.AddOrUpdateAsync("", gameData);

        // Reduce ticket from guest
        user.Ticket--;
        user.XP++;

        await userRepository.UpdateUserByIdAsync(userId,
            Builders<User>.Update
                .Set(x => x.Ticket, user.Ticket)
                .Set(x => x.UpdatedAt, DateTime.UtcNow));

        User hostUser = await userRepository.GetUserByIdAsync(gameData.PlayerRoomGuest.UserId);

        // Notify the host
        await Task.Delay(TimeSpan.FromSeconds(5));
        if (GameHub.TryGetConnectionId(gameData.PlayerRoomHost.UserId, out var connectionId))
        {
            await hubContext.Clients.User(gameData.PlayerRoomHost.UserId).SendAsync("JoinGame", gameData);
        }
        else
        {
            // Handle offline user (store notification, etc.)
        }

        return (user.Ticket, gameData);
    }

    public async Task SaveGameProgress(MainGameLogProgressRequestModel dto)
    {
        CoreBeeGameData coreBeeGameData = await coreBeeGameRedisRepository.GetAsync("", dto.GameId);
        if (coreBeeGameData == null) throw new Exception("Game not found");

        var usernamePlayed = dto.Round.Username;
        User userPlayed = await userRepository.GetUserByUsernameAsync(usernamePlayed);
        if (userPlayed == null) throw new InvalidOperationException($"User {usernamePlayed} not found.");

        // Prevent the user from playing again when its the opponents turn
        if (coreBeeGameData.RoundLogs.Count > 1 && coreBeeGameData.RoundLogs.Last().Username == usernamePlayed)
        {
            throw new InvalidOperationException("you played your turn, you should wait for your opponent");
        }

        // Update the rounds
        coreBeeGameData.RoundLogs.Add(dto.Round);
        await coreBeeGameRedisRepository.AddOrUpdateAsync("", coreBeeGameData);

        string turnedPlayerId = "";

        // If the room has a guest then inform them
        if (coreBeeGameData.PlayerRoomGuest != null)
        {
            // find who is going to recieve the notif
            if (userPlayed.Id == coreBeeGameData.PlayerRoomHost.UserId)
            {
                turnedPlayerId = coreBeeGameData.PlayerRoomGuest.UserId;
            }
            else
            {
                turnedPlayerId = coreBeeGameData.PlayerRoomHost.UserId;
            }
         
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
        // TODO if no one played anything there shouldne be any awards!

        CoreBeeGameData gameData = await coreBeeGameRedisRepository.GetAsync("", gameId);
        if (gameData == null) throw new Exception($"Game {gameId} not found");

        User winnerUser = await userRepository.GetUserByIdAsync(winnerUserId);
        if (winnerUser == null) throw new InvalidOperationException($"Winner {winnerUserId} not found.");

        string loserId = "";
        User loserUser = null;

        // If we have the other joined player, then he is the loser
        if (gameData.PlayerRoomGuest != null)
        { 
            if (winnerUserId == gameData.PlayerRoomHost.UserId)
            {
                loserId = gameData.PlayerRoomGuest.UserId;
            }
            else
            {
                loserId = gameData.PlayerRoomHost.UserId;
            }

            loserUser = await userRepository.GetUserByIdAsync(loserId);
        }

        MainGameHistory gameHistory = new MainGameHistory();
        gameHistory.GameId = gameId;
        gameHistory.WinnerName = winnerUser.Username;
        gameHistory.LoserName = loserUser?.Username ?? string.Empty; // If we there was no other player in the room we dont have any losers
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

        if (loserUser != null)
        {
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
}