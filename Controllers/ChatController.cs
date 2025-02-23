using Microsoft.AspNetCore.Mvc;
using Converse.Services.Chat;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Converse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _chatService.SendMessageAsync(request.Recipient, request.Sender, request.Message);
                return Ok(new { message = "Message Sent." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message from {request.Sender} to {request.Recipient}: {ex}");
                return StatusCode(500, "An error occurred while sending the message.");
            }
        }
    }

    public class SendMessageRequest
    {
        [Required]
        public string Sender { get; set; }

        [Required]
        public string Recipient { get; set; }

        [Required]
        [MinLength(1)]
        public string Message { get; set; }
    }
}