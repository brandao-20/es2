using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebAPI.Entities;

public partial class Utilizador
{
    public int UtilizadorId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;
    public string Email { get; set; } = null!;

    public string? GoogleToken { get; set; }

    public string? Telefone { get; set; }

    public string? Cargo { get; set; }

    public DateTime DataCriacao { get; set; }

    [JsonIgnore]
    public int? TipoUtilizadorId { get; set; }

    public virtual ICollection<RegistosPreco> RegistosPrecos { get; set; } = new List<RegistosPreco>();

    public virtual TipoUtilizador? TipoUtilizador { get; set; }
}
