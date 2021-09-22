using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SenderService.Models;
using SenderService.Repositories;

namespace SenderService.Services
{
    public class SenderService : IHostedService
    {
        private readonly EventBusSenderService _eventBusSenderService;
        private readonly IRedisRepository _redisRepository;
        private Timer _timer;

        public SenderService(EventBusSenderService eventBusSenderService, IRedisRepository redisRepository)
        {
            _eventBusSenderService = eventBusSenderService;
            _redisRepository = redisRepository;
        }

        private async void SendMessage(object state)
        {
            var root = new Root
            {
                Type = "type",
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now.ToString(),
                DeviceId = Guid.NewGuid().ToString(),
                UserId = Guid.NewGuid().ToString()
            };
            
            await _eventBusSenderService.SendMessage(root);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(SendMessage, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}