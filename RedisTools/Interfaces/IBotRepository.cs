namespace RedisTools.Interfaces;

public interface IBotRepository
{
    List<string> GetAllBotNames();
    List<string> GetAllBotsIds();
    Task<List<string>> GetAllBotsIdsAsync();
}