namespace EventHubService.Repositories
{
    public interface IRedisRepository
    {
        public void PushStringToList(string listName, string str);
        public void SetIntoHash(string hashName, string key, string value);
        public string GetFromHash(string hashName, string key);
    }
}