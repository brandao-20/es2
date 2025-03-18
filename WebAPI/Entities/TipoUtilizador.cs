using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebAPI.Entities;

public partial class TipoUtilizador
{
    public int TipoUtilizadorId { get; set; }

    public string Tipo { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Utilizador> Utilizadores { get; set; } = new List<Utilizador>();
}
