using System;
using EventHubService.Providers;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EventHubService.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly ILogger<RedisRepository> _logger;
        private readonly IDatabase _cache;

        public RedisRepository(IRedisProvider redisProvider, ILogger<RedisRepository> logger)
        {
            _logger = logger;
            _cache = redisProvider.GetDatabase();
        }

        public void PushStringToList(string listName, string str)
        {
             _cache.ListRightPush(listName, str);
        }

        public void SetIntoHash(string hashName, string key, string value)
        {
            _cache.HashSet(hashName, key, value);
        }
        
        public string GetFromHash(string hashName, string key)
        {
            return _cache.HashGet(hashName, key);
        }
    }
}