using StackExchange.Redis;

namespace RedisPooler
{
    public interface IConnectionPool
    {
        IConnectionMultiplexer GetConnection();
    }
}