using System;
using Microsoft.Extensions.Logging;
using SenderService.Providers;
using StackExchange.Redis;

namespace SenderService.Repositories
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

        public void AddToHash(string hashName, string key, string value)
        {
            _cache.HashSet(hashName, key, value);
        }
    }
}