using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebAPI.Entities;

public partial class Loja
{
    public int LojaId { get; set; }

    public string Nome { get; set; } = null!;

    public string Endereco { get; set; } = null!;

    [JsonIgnore]
    public int? LocalizacaoId { get; set; }

    public virtual Localizacao? Localizacao { get; set; }

    public virtual ICollection<RegistosPreco> RegistosPrecos { get; set; } = new List<RegistosPreco>();
}
