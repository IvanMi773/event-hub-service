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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

        private readonly RedisRepository _redisRepository;
        private readonly RootValidator _rootValidator;

        public EventHubReceiverService(IConfiguration configuration, ILogger<EventHubReceiverService> logger, RedisRepository redisRepository, RootValidator rootValidator)
        {
            _logger = logger;
            _redisRepository = redisRepository;
            _rootValidator = rootValidator;
            
            if (Configuration == null)
            {
                Configuration = configuration;
            }
        }

        public async Task ProcessEventHandler(ProcessEventArgs eventArgs)
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

        public Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            Console.WriteLine($"\tPartition '{ eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
            Console.WriteLine(eventArgs.Exception.Message);
            
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
            await _processor.StopProcessingAsync(cancellationToken);
        }
    }
}