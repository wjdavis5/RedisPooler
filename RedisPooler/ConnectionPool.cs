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
        public ConcurrentQueue<Lazy<Task<ConnectionMultiplexer>>> ConnectionQueue { get; }

        public ConnectionPool(int poolSize, ConfigurationOptions redisConfigurationOptions, bool useLazyInit = true,
            TextWriter redisTextWriterLog = null)
        {
            if(poolSize <= 0) throw new Exception("Must pass in a value larger than 0");
            RedisConfigurationOptions = redisConfigurationOptions ?? throw new ArgumentNullException(nameof(redisConfigurationOptions));
            UseLazyInit = useLazyInit;
            RedisTextWriterLog = redisTextWriterLog;
            ConnectionQueue = new ConcurrentQueue<Lazy<Task<ConnectionMultiplexer>>>();
            PoolSize = poolSize;
            BuildPool();
        }

        private async void BuildPool()
        {
            while(ConnectionQueue.Count < PoolSize)
            {
                var con = new Lazy<Task<ConnectionMultiplexer>>(LoadLazyConnection);
                if (!UseLazyInit)
                {
                    await con.Value;
                }
                ConnectionQueue.Enqueue(con);
            }
        }

        private Task<ConnectionMultiplexer> LoadLazyConnection()
        {
            return ConnectionMultiplexer.ConnectAsync(RedisConfigurationOptions, RedisTextWriterLog);
        }

        public IConnectionMultiplexer GetConnection()
        {
            Lazy<Task<ConnectionMultiplexer>> connection;
            while (!ConnectionQueue.TryDequeue(out connection))
            {
                Interlocked.Increment(ref NumSpinsGettingConnection);
                continue;
            }
            ConnectionQueue.Enqueue(connection);
            return connection.Value.Result;
        }
    }
}
