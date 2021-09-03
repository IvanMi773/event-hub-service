using System;
using EventHubService.Providers;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EventHubService.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly RedisProvider _redisConfig;
        private readonly ILogger<RedisRepository> _logger;

        public RedisRepository(RedisProvider redisConfig, ILogger<RedisRepository> logger)
        {
            _redisConfig = redisConfig;
            _logger = logger;
        }

        public void PushStringToList(string str)
        {
            IDatabase cache = _redisConfig.GetDatabase();
            _logger.LogInformation(cache.ListRightPush("roots", str).ToString());
        }
    }
}