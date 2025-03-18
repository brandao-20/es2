using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebAPI.Entities;

public partial class Produto
{
    public int ProdutoId { get; set; }

    public string Nome { get; set; } = null!;

    public string Marca { get; set; } = null!;

    public string? Descricao { get; set; }

    [JsonIgnore]
    public int CategoriaId { get; set; }

    public virtual Categoria? Categoria { get; set; } = null!;

    public virtual ICollection<RegistosPreco> RegistosPrecos { get; set; } = new List<RegistosPreco>();
}
