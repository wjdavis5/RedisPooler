using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationOptions();
            config.EndPoints.Add("127.0.0.1",6379);
            var connectionPooler = new RedisPooler.ConnectionPool(20,config,false);
            var nums = Enumerable.Range(1, 10000).ToList();
            nums.AsParallel().WithDegreeOfParallelism(5).ForAll(i =>
            {
                connectionPooler.GetConnection().GetDatabase().StringSet(i.ToString(), i.ToString());
            });
            Console.WriteLine(connectionPooler.NumSpinsGettingConnection);
        }
    }
}
