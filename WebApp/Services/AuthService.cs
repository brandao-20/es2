namespace WebApp.Services
{
    public class AuthService
    {
        public string UserName { get; set; } = "";
        public string Role { get; set; } = "";
        public string Token { get; set; } = "";

        // Armazena o ID do utilizador logado
        public int UserId { get; set; } = 0;

        public bool IsLoggedIn => !string.IsNullOrEmpty(Token);
        public bool IsAdmin => Role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
        public bool IsManager => Role.Equals("USER_MANAGER", StringComparison.OrdinalIgnoreCase);
        public bool IsUser => Role.Equals("USER", StringComparison.OrdinalIgnoreCase);

        public void Clear()
        {
            UserName = "";
            Role = "";
            Token = "";
            UserId = 0;
        }
    }
}
