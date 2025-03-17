namespace WebApp.Models
{
    public class Produto
    {
        public int ProdutoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public int CategoriaId { get; set; }
       
        public Categoria? Categoria { get; set; }
    }
}