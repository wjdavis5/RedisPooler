using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisPooler
{
    public interface IConnectionPool
    {
        IConnectionMultiplexer GetConnection();
        ConcurrentQueue<Lazy<Task<ConnectionMultiplexer>>> ConnectionQueue { get; }
    }
}