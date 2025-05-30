using System.Text.Json.Serialization;

namespace WebAPI.Entities
{
    public class Categoria
    {
        public int CategoriaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int? ParentId { get; set; }

        [JsonIgnore]
        public Categoria? Parent { get; set; }

        [JsonIgnore]
        public List<Categoria>? SubCategorias { get; set; }

        [JsonIgnore] 
        public List<Produto>? Produtos { get; set; }
    }
}
