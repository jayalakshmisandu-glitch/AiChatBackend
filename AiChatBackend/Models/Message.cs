using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AiChatBackend.Models
{
    public class ChatMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("chatId")]
        public string ChatId { get; set; } = null!;

        [BsonElement("userId")]
        public string UserId { get; set; } = null!;

        [BsonElement("role")]
        public string Role { get; set; } = null!; // "user" or "ai"

        [BsonElement("content")]
        public string Content { get; set; } = null!;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
