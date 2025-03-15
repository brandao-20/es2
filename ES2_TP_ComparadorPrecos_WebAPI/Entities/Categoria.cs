using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ES2_TP_ComparadorPrecos_WebAPI.Entities;

public partial class Categoria
{
    public int CategoriaId { get; set; }

    public string Nome { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
