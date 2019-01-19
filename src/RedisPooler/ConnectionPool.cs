using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisPooler
{
    public sealed class ConnectionPool : IConnectionPool
    {
        public ConfigurationOptions RedisConfigurationOptions { get; }
        public bool UseLazyInit { get; }
        public TextWriter RedisTextWriterLog { get; }
        public int PoolSize { get; }
        private ConcurrentQueue<Lazy<Task<ConnectionMultiplexer>>> ConnectionQueue { get; }

        public ConnectionPool(int poolSize, ConfigurationOptions redisConfigurationOptions, bool useLazyInit = true,
            TextWriter redisTextWriterLog = null)
        {
            RedisConfigurationOptions = redisConfigurationOptions;
            UseLazyInit = useLazyInit;
            RedisTextWriterLog = redisTextWriterLog;
            ConnectionQueue = new ConcurrentQueue<Lazy<Task<ConnectionMultiplexer>>>();
            PoolSize = poolSize;
            BuildPool();
        }

        private void BuildPool()
        {
            for (var i = 0; i < PoolSize; i++)
            {
                var lazyCon = new Lazy<Task<ConnectionMultiplexer>>(CreateRedisConnection);
                if (!UseLazyInit)
                {
                    var val = lazyCon.Value;
                }
                ConnectionQueue.Enqueue(lazyCon);
            }
        }

        private Task<ConnectionMultiplexer> CreateRedisConnection()
        {
            return ConnectionMultiplexer.ConnectAsync(RedisConfigurationOptions, RedisTextWriterLog);
        }

        public IConnectionMultiplexer GetConnection()
        {
            Lazy<Task<ConnectionMultiplexer>> connection = null;
            while (!ConnectionQueue.TryDequeue(out connection))
            {
                continue;
            }
            ConnectionQueue.Enqueue(connection);
            return connection.Value.GetAwaiter().GetResult();
        }
    }
}
