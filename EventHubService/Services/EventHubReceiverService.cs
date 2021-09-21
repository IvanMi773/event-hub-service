using System;
using System.Globalization;
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

        private BlobContainerClient _storageClient;
        private EventProcessorClient _processor;

        private readonly IConfiguration _configuration;
        private readonly ILogger<EventHubReceiverService> _logger;

        private readonly IRedisRepository _redisRepository;

        public EventHubReceiverService(IConfiguration configuration, ILogger<EventHubReceiverService> logger,
            IRedisRepository redisRepository)
        {
            _logger = logger;
            _redisRepository = redisRepository;

            _configuration = configuration;
        }

        private async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            var jsonStr = Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray());
            Root deserializedRoot = new ();
            try
            {
                deserializedRoot = JsonConvert.DeserializeObject<Root>(jsonStr);

                var timestamp = _redisRepository.GetFromHash("ingest-event-hub-hash", deserializedRoot?.Id);
                var eventTimestampDate = DateTime.Parse(deserializedRoot.Timestamp);
                var redisTimestampDate = DateTime.Parse(timestamp);

                if (DateTime.Compare(eventTimestampDate, redisTimestampDate) >= 0)
                {
                    _redisRepository.SetIntoHash("ingest-event-hub-hash", deserializedRoot.Id, deserializedRoot.Timestamp);
                    _redisRepository.PushStringToList("roots", jsonStr);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
            finally
            {
                await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
            }
        }

        private Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            _logger.LogWarning(
                $"\tPartition '{eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
            _logger.LogWarning(eventArgs.Exception.Message);

            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
            _storageClient = new BlobContainerClient(_configuration[BlobStorageConnectionStringKey],
                _configuration[BlobContainerNameKey]);
            _processor = new EventProcessorClient(_storageClient, consumerGroup,
                _configuration[EhubNamespaceConnectionStringKey], _configuration[EventHubNameKey]);

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