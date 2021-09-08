using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SenderService.Models;
using SenderService.Services;

namespace SenderService.Controllers
{
    public class SenderController : ControllerBase
    {
        private readonly EventBusSenderService _sender;

        public SenderController(EventBusSenderService sender)
        {
            _sender = sender;
        }

        [HttpPost("send")]
        public async Task<string> Send([FromBody] Root root)
        {
            await _sender.SendMessage(root);
            
            return "ok";
        }
    }
}