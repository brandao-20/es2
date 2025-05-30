namespace WebApp.Models
{
    public class Categoria
    {
        public int CategoriaId { get; set; }

        public string Nome { get; set; } = string.Empty;

        public int? ParentId { get; set; } // Adicionando ParentId

        public List<Categoria> SubCategorias { get; set; } = new List<Categoria>(); // Para exibir hierarquia no frontend
    }
}
