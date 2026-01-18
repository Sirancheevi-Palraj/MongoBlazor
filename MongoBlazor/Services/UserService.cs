using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoBlazor.Model;

namespace MongoBlazor.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var db = client.GetDatabase(settings.Value.Database);
            _users = db.GetCollection<User>(settings.Value.User);
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await _users.Find(_ => true)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _users.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UsernameExistsAsync(string username, string? ignoreUserId = null)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Username, username);

            if (!string.IsNullOrEmpty(ignoreUserId))
                filter &= Builders<User>.Filter.Ne(x => x.Id, ignoreUserId);

            return await _users.Find(filter).AnyAsync();
        }

        public async Task CreateAsync(User user)
        {
            await _users.InsertOneAsync(user);
        }

        public async Task UpdateAsync(User user)
        {
            await _users.ReplaceOneAsync(x => x.Id == user.Id, user);
        }

        public async Task DeleteAsync(string id)
        {
            await _users.DeleteOneAsync(x => x.Id == id);
        }
        public async Task<bool> ChangePasswordByUsernameAsync(string username, string oldPassword, string newPassword)
        {
            var user = await _users.Find(x => x.Username == username && x.IsActive).FirstOrDefaultAsync();
            if (user == null)
                return false;

            if (user.Password != oldPassword)
                return false;

            user.Password = newPassword;
            await _users.ReplaceOneAsync(x => x.Id == user.Id, user);

            return true;
        }

    }
}
