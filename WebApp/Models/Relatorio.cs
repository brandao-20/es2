namespace WebApp.Models
{
    public class Relatorio
    {
        public int ProdutoId { get; set; }
        public string NomeProduto { get; set; } = "";
        public int LojaId { get; set; }
        public string NomeLoja { get; set; } = "";
        public decimal Preco { get; set; }
        public DateTime DataRegisto { get; set; }
        public int CategoriaId { get; set; }
        public string NomeCategoria { get; set; } = "";
    }
}
