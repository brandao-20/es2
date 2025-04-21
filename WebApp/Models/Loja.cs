namespace WebApp.Models
{
    public class Loja
    {
        public int LojaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Supermercado { get; set; } = string.Empty; // Novo campo
        public string Endereco { get; set; } = string.Empty;
        public int? LocalizacaoId { get; set; }
        public Localizacao? Localizacao { get; set; }
    }
}
