using AiChatBackend.Models;
using Google.Protobuf;
using MongoDB.Driver;

namespace AiChatBackend.DAL
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString = configuration["MongoDbSettings:ConnectionString"];
            var databaseName = configuration["MongoDbSettings:DatabaseName"];

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Chat> Chats => _database.GetCollection<Chat>("Chats");
        public IMongoCollection<ChatMessage> Messages => _database.GetCollection<ChatMessage>("Messages");
    }
}
