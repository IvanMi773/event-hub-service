namespace EventHubService.Repositories
{
    public interface IRedisRepository
    {
        void PushStringToList(string str);
    }
}