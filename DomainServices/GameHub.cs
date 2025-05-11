using DnsClient.Internal;
using DomainModels.Models.Game;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace DomainModels.Models;

public class GameHub : Hub
{
    // better to use redis and concurrent dic
    private static readonly ConcurrentDictionary<string, string> _userConnections = new();
    //private readonly ILogger logger;

    //public GameHub(ILogger logger)
    //{
    //    this.logger = logger;
    //}

    public override async Task OnConnectedAsync()
    {
        // better to use lock
        var userId = Context.GetHttpContext().Request.Query["userIdData"];
        var deviceId = Context.GetHttpContext().Request.Query["deviceIdData"];

        _userConnections.TryAdd(userId, Context.ConnectionId);
        //logger.LogInformation($"userid {userId} is now connected to the hub server...");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.UserIdentifier;
        _userConnections.TryRemove(userId, out _);
        await base.OnDisconnectedAsync(exception);
    }

    public static bool TryGetConnectionId(string userId, out string connectionId)
    {
        return _userConnections.TryGetValue(userId, out connectionId);
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
