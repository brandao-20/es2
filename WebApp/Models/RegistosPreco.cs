namespace WebApp.Models
{
    public class RegistosPreco
    {
        public int RegistoPrecoId { get; set; }
        public decimal Preco { get; set; }
        public DateTime DataRegisto { get; set; }
        public decimal Credibilidade { get; set; }
        public int TipoAcaoId { get; set; }
        public int ProdutoId { get; set; }
        public int LojaId { get; set; }
        public int UtilizadorId { get; set; }

        public Produto? Produto { get; set; }
        public Loja? Loja { get; set; }
        public string LojaNome => Loja?.Nome ?? "";
        public TipoAcao? TipoAcao { get; set; }
        public List<Comentario> Comentarios { get; set; } = new();
    }
}
