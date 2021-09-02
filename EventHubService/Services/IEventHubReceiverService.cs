using System.Threading.Tasks;

namespace EventHubService.Services
{
    public interface IEventHubReceiverService
    {
        Task ReceiveMessage();
    }
}