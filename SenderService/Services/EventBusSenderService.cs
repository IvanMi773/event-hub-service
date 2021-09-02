using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using SenderService.Models;
using EventData = Azure.Messaging.EventHubs.EventData;

namespace SenderService.Services
{
    public class EventBusSenderService
    {
        private const string ConnectionString =
            "Endpoint=sb://test-eventhub.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=7CuCEzEn0FC5GjSG5Xrft0HepzlT6ABX/Pgi9M6ixgI=";

        private const string EventHubName = "eventhub";

        private static EventHubProducerClient _producerClient;

        public async Task SendMessage(Root root)
        {
            _producerClient = new EventHubProducerClient(ConnectionString, EventHubName);

            using EventDataBatch eventBatch = await _producerClient.CreateBatchAsync();
            
            if (!eventBatch.TryAdd(new EventData(JsonSerializer.Serialize(root))))
            {
                throw new Exception($"Event is too large for the batch and cannot be sent.");
            }
            
            try
            {
                await _producerClient.SendAsync(eventBatch);
            }
            finally
            {
                await _producerClient.DisposeAsync();
            }
        }
    }
}