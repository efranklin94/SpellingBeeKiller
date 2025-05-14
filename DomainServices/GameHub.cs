using DnsClient.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace DomainModels.Models;

public class GameHub : Hub
{
    // better to use redis and concurrent dic
    private static readonly ConcurrentDictionary<string, string> _userConnections = new();
    private readonly ILogger<GameHub> logger;

    public GameHub(ILogger<GameHub> logger)
    {
        this.logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var userId = httpContext.Request.Query["userIdData"].ToString();

        if (!string.IsNullOrEmpty(userId))
        {
            lock (_userConnections)
            {
                _userConnections[userId] = Context.ConnectionId;
                logger.LogInformation($"userid : {userId} is now connected to the hubserver with the connectionId {Context.ConnectionId}");
            }
        }

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        string? userIdToRemove = null;

        foreach (var kvp in _userConnections)
        {
            if (kvp.Value == connectionId)
            {
                userIdToRemove = kvp.Key;
                logger.LogInformation($"userid : {userIdToRemove} has disconnected from the hubserver with the connectionId {connectionId}");

                break;
            }
        }

        if (userIdToRemove is not null)
            _userConnections.TryRemove(userIdToRemove, out _);

        return base.OnDisconnectedAsync(exception);
    }


    public static bool TryGetConnectionId(string userId, out string connectionId)
    {
        lock (_userConnections)
        {
            if (!string.IsNullOrEmpty(userId) && _userConnections.TryGetValue(userId, out var connId))
            {
                connectionId = connId;
                return true;
            }
            connectionId = null!;
            return false;
        }
    }


    //public async Task UpdateGameForUser()
    //{
    //    var userId = Context.GetHttpContext().Request.Query["userIdData"];

    //    await Clients.User(userId).SendAsync("UpdateGameForUser", new CoreBeeGameData { /* TEST */ });
    //}

    //public async Task NotifyMatchFound(string opponentId)
    //{
    //    await Clients.User(Context.UserIdentifier)
    //        .SendAsync("MatchFound", opponentId);
    //}

    //public async Task BroadcastGameState(string gameId, GameState state)
    //{
    //    await Clients.Group(gameId).SendAsync("GameStateUpdated", state);
    //}

    // In join use
    //await Groups.AddToGroupAsync(Context.ConnectionId, userId);

    // In game finish use
    // await Groups.RemoveFromGroupAsync(Context.ConnectionId, currentGameId);

}
