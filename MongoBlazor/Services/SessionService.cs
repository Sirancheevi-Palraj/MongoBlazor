using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MongoBlazor.Model;

namespace MongoBlazor.Services
{
    public class SessionService
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private const string SessionKey = "UserSession";

        public SessionService(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public async Task<UserSession?> GetSessionAsync()
        {
            try
            {
                var result = await _sessionStorage.GetAsync<UserSession>(SessionKey);
                return result.Success ? result.Value : null;
            }
            catch
            {
                return null;
            }
        }

        public async Task SetSessionAsync(UserSession session)
        {
            await _sessionStorage.SetAsync(SessionKey, session);
        }

        public async Task ClearSessionAsync()
        {
            await _sessionStorage.DeleteAsync(SessionKey);
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var session = await GetSessionAsync();
            return session != null;
        }
    }
}