namespace EventHubService.Repositories
{
    public interface IRedisRepository
    {
        void PushStringToList(string listName, string str);
    }
}