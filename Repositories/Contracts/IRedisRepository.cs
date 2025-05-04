namespace Repositories.Contracts;

public interface IRedisRepository<T> where T : class
{
    Task AddOrUpdateAsync(string userId, T data, TimeSpan? expiry = null);
    Task<T> GetAsync(string userId, string gameId);
    Task RemoveAsync(string userId, string gameId);
    Task<IEnumerable<T>> GetAllForUserAsync(string userId);
}
