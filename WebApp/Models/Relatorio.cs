namespace WebApp.Models
{
    public class Relatorio
    {
        public int Id { get; set; }
        public string NomeProduto { get; set; }
        public int ProdutoId { get; set; }
        public string NomeLoja { get; set; }
        public int LojaId { get; set; }
        public decimal Preco { get; set; }
        public DateTime Data { get; set; }
        public string NomeCategoria { get; set; }
        public int CategoriaId { get; set; }
    }
}