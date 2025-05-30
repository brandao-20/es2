namespace WebApp.Models
{
    public class Favorito
    {
        public int FavoritoId { get; set; }
        public int UtilizadorId { get; set; }
        public int ProdutoId { get; set; }
        public Utilizador? Utilizador { get; set; }
        public Produto? Produto { get; set; }
    }
}
