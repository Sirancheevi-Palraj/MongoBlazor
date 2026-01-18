namespace MongoBlazor.Model
{
    public class AuthenticationSettings
    {
        public bool IsLoginEnabled { get; set; }
        public int SessionTimeoutMinutes { get; set; } = 30;
    }
}
