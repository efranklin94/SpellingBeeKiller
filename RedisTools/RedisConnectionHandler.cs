using StackExchange.Redis;

namespace RedisTools;

public class RedisConnectionHandler
{
    private readonly Lazy<ConnectionMultiplexer> Connection;

    public RedisConnectionHandler(string connectionString)
    {
        ConfigurationOptions options = ConfigurationOptions.Parse(connectionString);

        Connection = new Lazy<ConnectionMultiplexer>(() =>
            ConnectionMultiplexer.Connect(options)
        );
    }

    public ConnectionMultiplexer GetConnection() => Connection.Value;

    public IServer GetServer
    {
        get
        {
            var endpoints = GetConnection().GetEndPoints();
            return GetConnection().GetServer(endpoints.First());
        }
    }
}
