using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoBlazor.Model
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("PasswordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("Email")]
        public string? Email { get; set; }

        [BsonElement("FullName")]
        public string? FullName { get; set; }

        [BsonElement("Role")]
        public string Role { get; set; } = "User";

        [BsonElement("IsActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("LastLoginAt")]
        public DateTime? LastLoginAt { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class UserSession
    {
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public DateTime LoginTime { get; set; }
    }
}