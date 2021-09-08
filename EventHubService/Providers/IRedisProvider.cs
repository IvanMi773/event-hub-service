using StackExchange.Redis;

namespace EventHubService.Providers
{
    public interface IRedisProvider
    {
        public IDatabase GetDatabase();
    }
}