using StackExchange.Redis;

namespace SenderService.Providers
{
    public interface IRedisProvider
    {
        public IDatabase GetDatabase();
    }
}