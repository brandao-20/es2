using System.Text.Json.Serialization;

namespace ES2_TP_ComparadorPrecos_BACKEND.models;

public class Produto
{
    public int ProdutoId { get; set; }
    public string Nome { get; set; } = null!;
    public string Marca { get; set; } = null!;
    public string Descricao { get; set; } = null!;

    // FK
    public int CategoriaId { get; set; }
    
    [JsonIgnore]
    public Categoria? Categoria { get; set; }
}