using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using RedisPooler;
using StackExchange.Redis;

namespace ConnectionPoolTest
{
    public class Tests
    {
        [Test]
        public void Throws_if_configuration_is_null()
        {
            Action act = () =>
            {
                var pooler = new ConnectionPool(4, null);
            };
            act.Should().Throw<ArgumentNullException>("teh config was null!");
        }

        [Test]
        public void Throws_if_poolsize_is_less_than_one()
        {
            Action act = () =>
            {
                var pooler = new ConnectionPool(-4, null);
            };
            act.Should().Throw<Exception>("the pool size was less than 1!");
        }

    }
}