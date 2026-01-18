using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoBlazor.Model;

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

            // ✅ Plain password
            if (user.Password != password)
                return null;

            var update = Builders<User>.Update.Set(u => u.LastLoginAt, DateTime.UtcNow);
            await _users.UpdateOneAsync(u => u.Id == user.Id, update);

            return user;
        }

        public async Task<bool> CreateUserAsync(User user, string password)
        {
            var existingUser = await _users.Find(u => u.Username == user.Username).FirstOrDefaultAsync();
            if (existingUser != null)
                return false;

            user.Password = password; // ✅ plain
            user.CreatedAt = DateTime.UtcNow;

            await _users.InsertOneAsync(user);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
                return false;

            if (user.Password != oldPassword)
                return false;

            var update = Builders<User>.Update.Set(u => u.Password, newPassword);
            await _users.UpdateOneAsync(u => u.Id == userId, update);

            return true;
        }

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
