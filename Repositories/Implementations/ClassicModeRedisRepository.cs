using DomainModels.Models.Game;
using MessagePack;
using Repositories.Contracts;
using StackExchange.Redis;

namespace Repositories.Implementations;

public class ClassicModeRedisRepository : IRedisRepository<ClassicModeData>
{
    private readonly IDatabase _redis;
    private const string Prefix = "classicMode";

    public ClassicModeRedisRepository(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    public async Task AddOrUpdateAsync(string userId, ClassicModeData data, TimeSpan? expiry = null)
    {
        var key = $"{Prefix}:{userId}:{data.GameId}";
        var value = MessagePackSerializer.Serialize(data);
        await _redis.StringSetAsync(key, value, expiry ?? TimeSpan.FromHours(48));
    }

    public async Task<ClassicModeData> GetAsync(string userId, string gameId)
    {
        var key = $"{Prefix}:{userId}:{gameId}";
        var value = await _redis.StringGetAsync(key);
        return value.HasValue ? MessagePackSerializer.Deserialize<ClassicModeData>(value) : null;
    }

    public async Task RemoveAsync(string userId, string gameId)
    {
        var key = $"{Prefix}:{userId}:{gameId}";
        await _redis.KeyDeleteAsync(key);
    }

    public async Task<IEnumerable<ClassicModeData>> GetAllForUserAsync(string userId)
    {
        var pattern = $"{Prefix}:{userId}:*";
        return await GetByPattern(pattern);
    }

    private async Task<IEnumerable<ClassicModeData>> GetByPattern(string pattern)
    {
        var server = _redis.Multiplexer.GetServer(_redis.Multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: pattern);

        var results = new List<ClassicModeData>();
        foreach (var key in keys)
        {
            var value = await _redis.StringGetAsync(key);
            results.Add(MessagePackSerializer.Deserialize<ClassicModeData>(value));
        }
        return results;
    }
}
