namespace WebApp.Services
{
    public class AuthService
    {
        public string UserName { get; set; } = "";
        public string Role { get; set; } = "";
        public string Token { get; set; } = "";

        public bool IsLoggedIn => !string.IsNullOrEmpty(Token);
        public bool IsAdmin => Role.Equals("ADMIN", System.StringComparison.OrdinalIgnoreCase);
        public bool IsManager => Role.Equals("USER_MANAGER", System.StringComparison.OrdinalIgnoreCase);
        public bool IsUser => Role.Equals("USER", System.StringComparison.OrdinalIgnoreCase);

        public void Clear()
        {
            UserName = "";
            Role = "";
            Token = "";
        }
    }
}
