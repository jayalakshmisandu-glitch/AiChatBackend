using AiChatBackend.DTOs;
using AiChatBackend.Sevices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AiChatBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? User.Identity?.Name
                   ?? throw new UnauthorizedAccessException();
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListChats()
        {
            var userId = GetUserId();
            var chats = await _chatService.GetChatsAsync(userId);
            return Ok(new { chats });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequestDto dto)
        {
            var userId = GetUserId();
            var chat = await _chatService.CreateChatAsync(userId, dto.Title);
            return Ok(new { chat });
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ChatId))
                return BadRequest("ChatId is required");
            if (string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest("Message is required");

            var userId = GetUserId();
            var response = await _chatService.SendMessageAsync(userId, dto.ChatId, dto.Message);

            return Ok(new ChatResponseDto { Response = response });
        }

        [HttpGet("{chatId}")]
        public async Task<IActionResult> GetChat(string chatId)
        {
            var userId = GetUserId();
            var chat = await _chatService.GetChatAsync(chatId, userId);
            if (chat == null) return NotFound("Chat not found");

            var messages = await _chatService.GetMessagesAsync(chatId, userId);
            return Ok(new { chat, messages });
        }

        [HttpPut("{chatId}/title")]
        public async Task<IActionResult> RenameChat(string chatId, [FromBody] UpdateChatTitleRequestDto dto)
        {
            var userId = GetUserId();
            var success = await _chatService.RenameChatAsync(chatId, userId, dto.Title);
            if (!success) return NotFound("Chat not found");
            return Ok(new { message = "Chat renamed" });
        }

        [HttpDelete("{chatId}")]
        public async Task<IActionResult> DeleteChat(string chatId)
        {
            var userId = GetUserId();
            var deleted = await _chatService.DeleteChatAsync(chatId, userId);
            if (!deleted) return NotFound("Chat not found");
            return Ok(new { message = "Chat deleted" });
        }
    }
}