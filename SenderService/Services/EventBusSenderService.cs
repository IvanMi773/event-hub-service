using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SenderService.Models;
using EventData = Azure.Messaging.EventHubs.EventData;

namespace SenderService.Services
{
    public class EventBusSenderService : IDisposable
    {
        private const string EhubNamespaceConnectionStringKey = "EhubNamespaceConnectionString";
        private const string EventHubNameKey = "EventHubName";
        private readonly IConfiguration _configuration;
        private readonly EventHubProducerClient _producerClient;
        private readonly ILogger<EventBusSenderService> _logger;

        public EventBusSenderService(IConfiguration configuration, ILogger<EventBusSenderService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _producerClient = new EventHubProducerClient(
                _configuration[EhubNamespaceConnectionStringKey], 
                _configuration[EventHubNameKey]
            );
        }

        public async Task SendMessage(Root root)
        {
            using EventDataBatch eventBatch = await _producerClient.CreateBatchAsync();
            
            if (!eventBatch.TryAdd(new EventData(JsonSerializer.Serialize(root))))
            {
                throw new Exception($"Event is too large for the batch and cannot be sent.");
            }
            
            try
            {
                await _producerClient.SendAsync(eventBatch);
            }
            catch (Exception)
            {
                _logger.LogError("Error while sending messages");
            }
        }

        public async void Dispose()
        {
            await _producerClient.DisposeAsync();
        }
    }
}