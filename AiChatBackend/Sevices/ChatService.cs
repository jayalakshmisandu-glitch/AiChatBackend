using AiChatBackend.DAL;
using AiChatBackend.Models;
using AiChatBackend.Sevices;
using MongoDB.Driver;

namespace AiChatBackend.Sevices
{
    public class ChatService
    {
        private readonly MongoDbContext _context;
        private readonly GeminiService _gemini;

        public ChatService(MongoDbContext context, GeminiService gemini)
        {
            _context = context;
            _gemini = gemini;
        }

        public async Task<List<Chat>> GetChatsAsync(string userId)
        {
            return await _context.Chats
                .Find(x => x.UserId == userId)
                .SortByDescending(x => x.UpdatedAt)
                .ToListAsync();
        }

        public async Task<Chat?> GetChatAsync(string chatId, string userId)
        {
            return await _context.Chats
                .Find(x => x.Id == chatId && x.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<Chat> CreateChatAsync(string userId, string? title = null)
        {
            var chat = new Chat
            {
                UserId = userId,
                Title = string.IsNullOrWhiteSpace(title) ? "New Chat" : title,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Chats.InsertOneAsync(chat);
            return chat;
        }

        public async Task<bool> DeleteChatAsync(string chatId, string userId)
        {
            var result = await _context.Chats.DeleteOneAsync(x => x.Id == chatId && x.UserId == userId);
            return result.DeletedCount > 0;
        }

        public async Task<bool> RenameChatAsync(string chatId, string userId, string title)
        {
            var update = Builders<Chat>.Update
                .Set(x => x.Title, title)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Chats.UpdateOneAsync(
                x => x.Id == chatId && x.UserId == userId,
                update);

            return result.ModifiedCount > 0;
        }

        public async Task<List<ChatMessage>> GetMessagesAsync(string chatId, string userId)
        {
            return await _context.Messages
                .Find(x => x.ChatId == chatId && x.UserId == userId)
                .SortBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<string> SendMessageAsync(string userId, string chatId, string message)
        {
            var chat = await GetChatAsync(chatId, userId);
            if (chat == null)
                throw new InvalidOperationException("Chat not found");

            var userMessage = new ChatMessage
            {
                ChatId = chatId,
                UserId = userId,
                Role = "user",
                Content = message,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Messages.InsertOneAsync(userMessage);

            if (chat.Title == "New Chat")
            {
                var title = message.Length <= 40 ? message : message.Substring(0, 40);
                await RenameChatAsync(chatId, userId, title);
            }

            var aiResponse = await _gemini.GetResponseAsync(message);

            var aiMessage = new ChatMessage
            {
                ChatId = chatId,
                UserId = userId,
                Role = "ai",
                Content = aiResponse,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Messages.InsertOneAsync(aiMessage);

            await _context.Chats.UpdateOneAsync(
                x => x.Id == chatId && x.UserId == userId,
                Builders<Chat>.Update.Set(x => x.UpdatedAt, DateTime.UtcNow));

            return aiResponse;
        }
    }
}