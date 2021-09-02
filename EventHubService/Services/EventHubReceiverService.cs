using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using EventHubService.Models;
using EventHubService.Repositories;
using EventHubService.Services.Validators;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventHubService.Services
{
    public class EventHubReceiverService : IHostedService
    {
        private const string EhubNamespaceConnectionString = "Endpoint=sb://test-eventhub.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=7CuCEzEn0FC5GjSG5Xrft0HepzlT6ABX/Pgi9M6ixgI=";
        private const string EventHubName = "eventhub";
        private const string BlobStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=teststorage32138;AccountKey=OtSjkQZr6PjDw0yi0C0jwNOba5cFlu4A0CD5MNSmiJ7/zGpPbhK6yBZeBgTS8S49OYXgXW3EBf29RFrXLRn3Tw==;EndpointSuffix=core.windows.net";
        private const string BlobContainerName = "test-container";
        private static BlobContainerClient _storageClient;      
        private static EventProcessorClient _processor;

        private readonly ILogger<EventHubReceiverService> _logger;

        private readonly RedisRepository _redisRepository;
        private readonly RootValidator _rootValidator;

        public EventHubReceiverService(ILogger<EventHubReceiverService> logger, RedisRepository redisRepository, RootValidator rootValidator)
        {
            _logger = logger;
            _redisRepository = redisRepository;
            _rootValidator = rootValidator;
        }

        private async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            var jsonStr = Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray());
            var receivedModel = JsonSerializer.Deserialize<Root>(jsonStr);

            if (_rootValidator.Validate(receivedModel))
            {
                _logger.LogInformation("Good object was received:\n{name}", jsonStr);
                _redisRepository.PushStringToList(jsonStr);
            }
            else
            {
                _logger.LogWarning("Invalid object was received:\n{name}", jsonStr);
            }

            await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
        }

        private Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            Console.WriteLine($"\tPartition '{ eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
            Console.WriteLine(eventArgs.Exception.Message);
            
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
            _storageClient = new BlobContainerClient(BlobStorageConnectionString, BlobContainerName);
            _processor = new EventProcessorClient(_storageClient, consumerGroup, EhubNamespaceConnectionString, EventHubName);

            _processor.ProcessEventAsync += ProcessEventHandler;
            _processor.ProcessErrorAsync += ProcessErrorHandler;
            _processor.StartProcessingAsync(cancellationToken);
            
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _processor.StopProcessingAsync();
        }
    }
}