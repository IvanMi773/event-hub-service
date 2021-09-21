namespace SenderService.Repositories
{
    public interface IRedisRepository
    {
        public void AddToHash(string hashName, string key, string value);
    }
}