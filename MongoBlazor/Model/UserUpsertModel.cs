namespace MongoBlazor.Model
{
    public class UserUpsertModel
    {
        public string? Id { get; set; }
        public string Username { get; set; } = "";
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; } = true;

        // ✅ only needed in Create / Change password
        public string Password { get; set; } = "";
    }
}
