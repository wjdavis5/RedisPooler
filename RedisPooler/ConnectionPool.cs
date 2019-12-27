using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisPooler
{
    public sealed class ConnectionPool : IConnectionPool
    {
        public long NumSpinsGettingConnection = 0;
        public ConfigurationOptions RedisConfigurationOptions { get; }
        public bool UseLazyInit { get; }
        public TextWriter RedisTextWriterLog { get; }
        public int PoolSize { get; }
        private ConcurrentQueue<IConnectionMultiplexer> ConnectionQueue { get; }

        public ConnectionPool(int poolSize, ConfigurationOptions redisConfigurationOptions, bool useLazyInit = true,
            TextWriter redisTextWriterLog = null)
        {
            if(poolSize <= 0) throw new Exception("Must pass in a value larger than 0");
            RedisConfigurationOptions = redisConfigurationOptions ?? throw new ArgumentNullException(nameof(redisConfigurationOptions));
            UseLazyInit = useLazyInit;
            RedisTextWriterLog = redisTextWriterLog;
            ConnectionQueue = new ConcurrentQueue<IConnectionMultiplexer>();
            PoolSize = poolSize;
            BuildPool();
        }

        private async void BuildPool()
        {
            for (var i = 0; i < PoolSize; i++)
            {
                var con = await CreateRedisConnection();
                ConnectionQueue.Enqueue(con);
            }
        }

        private Task<ConnectionMultiplexer> CreateRedisConnection()
        {
            return ConnectionMultiplexer.ConnectAsync(RedisConfigurationOptions, RedisTextWriterLog);
        }

        public IConnectionMultiplexer GetConnection()
        {
            IConnectionMultiplexer connection;
            while (!ConnectionQueue.TryDequeue(out connection))
            {
                Interlocked.Increment(ref NumSpinsGettingConnection);
                continue;
            }
            ConnectionQueue.Enqueue(connection);
            return connection;
        }
    }
}
