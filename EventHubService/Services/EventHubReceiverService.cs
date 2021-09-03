using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using EventHubService.Models;
using EventHubService.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EventHubService.Services
{
    public class EventHubReceiverService : IHostedService
    {
        private const string EhubNamespaceConnectionStringKey = "EhubNamespaceConnectionString";
        private const string EventHubNameKey = "EventHubName";
        private const string BlobStorageConnectionStringKey = "BlobStorageConnectionString";
        private const string BlobContainerNameKey = "BlobContainerName";
        
        private static BlobContainerClient _storageClient;      
        private static EventProcessorClient _processor;

        private static IConfiguration Configuration { get; set; }
        private readonly ILogger<EventHubReceiverService> _logger;

        private readonly IRedisRepository _redisRepository;

        public EventHubReceiverService(IConfiguration configuration, ILogger<EventHubReceiverService> logger, IRedisRepository redisRepository)
        {
            _logger = logger;
            _redisRepository = redisRepository;
            
            if (Configuration == null)
            {
                Configuration = configuration;
            }
        }

        public async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            var jsonStr = Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray());
            
            try
            {
                var receivedModel = JsonConvert.DeserializeObject<Root>(jsonStr);
                _logger.LogInformation("Good object was received:\n{name}", jsonStr);

                try
                {
                    _redisRepository.PushStringToList(jsonStr);
                    await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
                }
                catch (Exception)
                {
                    _logger.LogWarning("Invalid object was received:\n{name}", jsonStr);
                }
            }
            catch (Exception)
            {
                _logger.LogWarning("Invalid object was received:\n{name}", jsonStr);
                
                await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
            }
        }
        
        public Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            _logger.LogWarning($"\tPartition '{ eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
            _logger.LogWarning(eventArgs.Exception.Message);
            
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
            _storageClient = new BlobContainerClient(Configuration[BlobStorageConnectionStringKey], Configuration[BlobContainerNameKey]);
            _processor = new EventProcessorClient(_storageClient, consumerGroup, Configuration[EhubNamespaceConnectionStringKey], Configuration[EventHubNameKey]);

            _processor.ProcessEventAsync += ProcessEventHandler;
            _processor.ProcessErrorAsync += ProcessErrorHandler;
            _processor.StartProcessingAsync(cancellationToken);
            
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _processor?.StopProcessingAsync(cancellationToken);
        }
    }
}