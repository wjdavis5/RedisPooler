# RedisPooler
A library to provide pooled IConnectionMultiplexers to your appliation

[![Build Status](https://wjdavis5.visualstudio.com/RedisPooler/_apis/build/status/RedisPooler-CI?branchName=master)](https://wjdavis5.visualstudio.com/RedisPooler/_build/latest?definitionId=3&branchName=master)

# Why
We were plauged, as many are, by time out exceptions `StackExchange.Redis.RedisTimeoutException` 
We found that the easiest way to address this issue was to implement a pool of IConnectionMultiplexors.

In our particular case the problem was not server (redis) side, it was able to keep up just fine. But locally the io threads used internal to the StackExchange.Redis library were the bottleneck. I'd suspect that most people are running into the same thing, and this is an easy way to solve the problem.

# Get it:
https://www.nuget.org/packages/RedisPooler/

# Example

```
var config = new ConfigurationOptions();
            config.EndPoints.Add("127.0.0.1",6379);
            var connectionPooler = new RedisPooler.ConnectionPool(20,config,false);
            var nums = Enumerable.Range(1, 10000).ToList();
            nums.AsParallel().WithDegreeOfParallelism(5).ForAll(i =>
            {
                connectionPooler.GetConnection().GetDatabase().StringSet(i.ToString(), i.ToString());
            });
            Console.WriteLine(connectionPooler.NumSpinsGettingConnection);
```
