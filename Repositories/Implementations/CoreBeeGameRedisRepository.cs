using DomainModels.Models.Game;
using MessagePack;
using Repositories.Contracts;
using StackExchange.Redis;

namespace Repositories.Implementations;

// CoreBeeGameRedisService.cs
public class CoreBeeGameRedisRepository : IRedisRepository<CoreBeeGameDataDb>
{
    private readonly IDatabase _redis;
    private const string Prefix = "corebee";

    public CoreBeeGameRedisRepository(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    public async Task AddOrUpdateAsync(string userId, CoreBeeGameDataDb data, TimeSpan? expiry = null)
    {
        var key = $"{Prefix}:{data.GameId}";
        var value = MessagePackSerializer.Serialize(data);
        await _redis.StringSetAsync(key, value, expiry ?? TimeSpan.FromHours(48));
    }

    public async Task<CoreBeeGameDataDb> GetAsync(string userId, string gameId)
    {
        var key = $"{Prefix}:{gameId}";
        var value = await _redis.StringGetAsync(key);
        return value.HasValue ? MessagePackSerializer.Deserialize<CoreBeeGameDataDb>(value) : null;
    }

    public async Task RemoveAsync(string userId, string gameId)
    {
        var key = $"{Prefix}:{gameId}";
        await _redis.KeyDeleteAsync(key);
    }

    public async Task<IEnumerable<CoreBeeGameDataDb>> GetAllForUserAsync(string userId)
    {
        var pattern = $"{Prefix}:*";

        var result = await GetByPattern(pattern);
        result = result.Where(g => g.PlayerRoomHostId == userId || g.PlayerRoomGuestId == userId);
        return result;
    }

    private async Task<IEnumerable<CoreBeeGameDataDb>> GetByPattern(string pattern)
    {
        var server = _redis.Multiplexer.GetServer(_redis.Multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: pattern);

        var results = new List<CoreBeeGameDataDb>();
        foreach (var key in keys)
        {
            var value = await _redis.StringGetAsync(key);
            results.Add(MessagePackSerializer.Deserialize<CoreBeeGameDataDb>(value));
        }
        return results;
    }
}
