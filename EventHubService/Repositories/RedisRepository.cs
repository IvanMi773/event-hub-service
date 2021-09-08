using System;
using EventHubService.Providers;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EventHubService.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly RedisProvider _redisProvider;
        private readonly ILogger<RedisRepository> _logger;
        private readonly IDatabase cache;

        public RedisRepository(RedisProvider redisProvider, ILogger<RedisRepository> logger)
        {
            _redisProvider = redisProvider;
            _logger = logger;
            cache = _redisProvider.GetDatabase();
        }

        public void PushStringToList(string listName, string str)
        {
            _logger.LogInformation("Count of elements in list: " + cache.ListRightPush(listName, str));
        }
    }
}