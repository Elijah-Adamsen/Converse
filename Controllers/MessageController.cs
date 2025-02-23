using Microsoft.AspNetCore.Mvc;
using Converse.Services.Message;
using Converse.Models;
using System.Threading.Tasks;

namespace Converse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly MessageService _messageService;

        public MessageController(MessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveMessage([FromBody] MessageData message)
        {
            if (string.IsNullOrWhiteSpace(message.SenderPhone) ||
                string.IsNullOrWhiteSpace(message.Content) ||
                string.IsNullOrWhiteSpace(message.ReceiverPhone))
            {
                return BadRequest("All fields are required.");
            }

            message.SentAt = DateTime.UtcNow;
            var success = await _messageService.SaveAndSendMessageAsync(message);
            return success ? Ok("Message Saved.") : StatusCode(500, "Failed to save message.");
        }

        [HttpGet("history/{user1}/{user2}")]
        public async Task<IActionResult> GetMessageHistory(string user1, string user2)
        {
            var history = await _messageService.GetMessageHistoryAsync(user1, user2);

            if (history == null || history.Count == 0)
            {
                return NotFound("No message history found.");
            }

            return Ok(history);
        }
    }
}