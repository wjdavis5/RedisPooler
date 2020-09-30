using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisPooler;
using StackExchange.Redis;

namespace ConnectionPooler.Test.Integration
{
    [TestClass]
    public class ConnectionPoolTests
    {
        [TestMethod]
        public void Returns_Successful_Connection_To_Localhost_Redis_6379()
        {
            var config = new ConfigurationOptions();
            config.EndPoints.Add("127.0.0.1", 6379);
            var arbitraryPoolSize = 2;
            var pooler = new ConnectionPool(arbitraryPoolSize, config);
            var connection = pooler.GetConnection();
            connection.Should().BeOfType<ConnectionMultiplexer>();
        }

        [TestMethod]
        public void Returns_Successful_Connection_To_Localhost_Redis_6379_And_Can_Run_Commands()
        {
            var config = new ConfigurationOptions();
            config.EndPoints.Add("127.0.0.1", 6379);
            var arbitraryPoolSize = 20;
            var pooler = new ConnectionPool(arbitraryPoolSize, config);
            var connection = pooler.GetConnection();
            connection.Should().BeOfType<ConnectionMultiplexer>();
            var db = connection.GetDatabase();

            Enumerable.Range(1, 100000).ToList().AsParallel().WithDegreeOfParallelism(10).ForAll(i =>
            {
                var arbitraryvalue = "ArbitraryValue";
                var arbitrarykeyname = $"ArbitraryKeyName:{i}";
                var setResult = db.StringSet(arbitrarykeyname, arbitraryvalue);
                setResult.Should().BeTrue();
                var getResult = db.StringGet(arbitrarykeyname).ToString();
                getResult.Should().Match(arbitraryvalue);
            });
        }
    }
}
