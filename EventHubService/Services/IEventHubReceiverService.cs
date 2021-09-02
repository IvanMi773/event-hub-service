using System.Threading.Tasks;
using Azure.Messaging.EventHubs.Processor;

namespace EventHubService.Services
{
    public interface IEventHubReceiverService
    {
        Task ProcessEventHandler(ProcessEventArgs eventArgs);
        Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs);
    }
}