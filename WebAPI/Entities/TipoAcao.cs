using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebAPI.Entities;

public partial class TipoAcao
{
    public int TipoAcaoId { get; set; }

    public string Tipo { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<RegistosPreco> RegistosPrecos { get; set; } = new List<RegistosPreco>();
}
