using System.ComponentModel.DataAnnotations;

namespace WebAPI.Entities
{
    public class Favorito
    {
        public int FavoritoId { get; set; }
        public int ProdutoId { get; set; }
        public int UtilizadorId { get; set; }

        // Campos de navegação não devem ser obrigatórios
        public Produto? Produto { get; set; }
        public Utilizador? Utilizador { get; set; }
    }
}