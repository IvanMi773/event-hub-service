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

        public SenderService(EventBusSenderService eventBusSenderService)
        {
            _eventBusSenderService = eventBusSenderService;
        }

        private async void SendMessage(string id)
        {
            var root = new Root
            {
                Type = "type",
                Id = id,
                Timestamp = DateTime.Now.ToString(),
                DeviceId = Guid.NewGuid().ToString(),
                UserId = Guid.NewGuid().ToString()
            };
            
            await _eventBusSenderService.SendMessage(root);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                for (int i = 0; i < 4000; i++)
                {
                    SendMessage("Id number 1 (selected)");
                    Thread.Sleep(300);
                }
            });
            
            Task.Run(() =>
            {
                for (int i = 0; i < 4000; i++)
                {
                    SendMessage("Id number 2");
                    Thread.Sleep(100);
                }
            });
            
            Task.Run(() =>
            {
                for (int i = 0; i < 4000; i++)
                {
                    SendMessage("Id number 3");
                    Thread.Sleep(100);
                }
            });
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}