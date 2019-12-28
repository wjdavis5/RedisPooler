using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisPooler;
using StackExchange.Redis;

namespace ConnectionPooler.Test.Unit
{
    [TestClass]
    public class ConnectionPoolTests
    {
        [TestMethod]
        public async Task Throws_if_configuration_is_null()
        {
            Action act = () =>
            {
                var pooler = new ConnectionPool(4, null);
            };
            act.Should().Throw<ArgumentNullException>("teh config was null!");
        }

        [TestMethod]
        public async Task Throws_if_poolsize_is_less_than_one()
        {
            Action act = () =>
            {
                var pooler = new ConnectionPool(-4, null);
            };
            act.Should().Throw<Exception>("the pool size was less than 1!");
        }

        [TestMethod]
        public async Task Pool_Has_Multiple_Items()
        {
            var mockConfig = new ConfigurationOptions();
            var arbitraryPoolSize = 2;
            var pooler = new ConnectionPool(arbitraryPoolSize, mockConfig);
            pooler.ConnectionQueue.Count.Should().Be(arbitraryPoolSize, "we told it to create 2 connections");
        }

        [TestMethod]
        public async Task Throws_argument_exception_when_config_doesnt_have_endpoints()
        {
            Action act = () =>
            {
                var mockConfig = new ConfigurationOptions();
                var arbitraryPoolSize = 2;
                var pooler = new ConnectionPool(arbitraryPoolSize, mockConfig);
                pooler.GetConnection();
            };
            act.Should().Throw<ArgumentException>("the config didnt have any endpoints");
        }
    }
}
