using System;
using EventHubService.Configuration;
using StackExchange.Redis;

namespace EventHubService.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly RedisConfig _redisConfig;

        public RedisRepository(RedisConfig redisConfig)
        {
            _redisConfig = redisConfig;
        }

        public void PushStringToList(string str)
        {
            IDatabase cache = _redisConfig.GetDatabase();
            Console.WriteLine("f");
            Console.WriteLine(cache.ListLeftPush("roots", str));
        }
    }
}