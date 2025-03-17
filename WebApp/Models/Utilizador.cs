namespace WebApp.Models
{
    public class Utilizador
    {
        public int UtilizadorId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? GoogleToken { get; set; }
        public string? Telefone { get; set; }
        public string? Cargo { get; set; }
        public DateTime DataCriacao { get; set; }
        public int? TipoUtilizadorId { get; set; }

        public TipoUtilizador? TipoUtilizador { get; set; }
    }
}