using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Extensions.Configuration;
using SenderService.Models;
using EventData = Azure.Messaging.EventHubs.EventData;

namespace SenderService.Services
{
    public class EventBusSenderService
    {
        private const string EhubNamespaceConnectionStringKey = "EhubNamespaceConnectionString";
        private const string EventHubNameKey = "EventHubName";
        
        private static IConfiguration Configuration { get; set; }

        private static EventHubProducerClient _producerClient;

        public EventBusSenderService(IConfiguration configuration)
        {
            if (Configuration == null)
            {
                Configuration = configuration;
            }
        }

        public async Task SendMessage(Root root)
        {
            _producerClient = new EventHubProducerClient(
                Configuration[EhubNamespaceConnectionStringKey], 
                Configuration[EventHubNameKey]
                );

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