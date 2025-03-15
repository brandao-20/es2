using System.Text.Json.Serialization;

namespace ES2_TP_ComparadorPrecos_BACKEND.models;

public class Categoria
{
    public int CategoriaId { get; set; }
    public string Nome { get; set; } = null!;

    [JsonIgnore] // Ignora a propriedade durante a serialização
    public ICollection<Produto>? Produtos { get; set; }
}