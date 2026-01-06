using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoBlazor.Model;
using System.Security.Cryptography;
using System.Text;

namespace MongoBlazor.Services
{
    public class AuthService
    {
        private readonly IMongoCollection<User> _users;
        private readonly AuthenticationSettings _authSettings;

        public AuthService(IOptions<MongoDbSettings> settings, IOptions<AuthenticationSettings> authSettings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.Database);
            _users = database.GetCollection<User>(settings.Value.User);
            _authSettings = authSettings.Value;
        }

        public bool IsLoginEnabled => _authSettings.IsLoginEnabled;

        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            var user = await _users.Find(u => u.Username == username && u.IsActive).FirstOrDefaultAsync();

            if (user == null)
                return null;

            var hashedPassword = HashPassword(password);
            if (user.PasswordHash != hashedPassword)
                return null;

            // Update last login time
            var update = Builders<User>.Update.Set(u => u.LastLoginAt, DateTime.UtcNow);
            await _users.UpdateOneAsync(u => u.Id == user.Id, update);

            return user;
        }

        public async Task<bool> CreateUserAsync(User user, string password)
        {
            try
            {
                // Check if username already exists
                var existingUser = await _users.Find(u => u.Username == user.Username).FirstOrDefaultAsync();
                if (existingUser != null)
                    return false;

                user.PasswordHash = HashPassword(password);
                user.CreatedAt = DateTime.UtcNow;

                await _users.InsertOneAsync(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
                return false;

            var oldHashedPassword = HashPassword(oldPassword);
            if (user.PasswordHash != oldHashedPassword)
                return false;

            var newHashedPassword = HashPassword(newPassword);
            var update = Builders<User>.Update.Set(u => u.PasswordHash, newHashedPassword);
            await _users.UpdateOneAsync(u => u.Id == userId, update);

            return true;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // Helper method to create initial admin user
        public async Task EnsureAdminUserExists()
        {
            var adminExists = await _users.Find(u => u.Username == "admin").AnyAsync();
            if (!adminExists)
            {
                var admin = new User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    FullName = "System Administrator",
                    Role = "Admin",
                    IsActive = true
                };
                await CreateUserAsync(admin, "Admin@123");
            }
        }
    }
}