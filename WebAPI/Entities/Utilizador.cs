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

    public string? GoogleId { get; set; }

    public string? Telefone { get; set; }

    public string? Cargo { get; set; }

    public DateTime DataCriacao { get; set; }

    [JsonIgnore]
    public int? TipoUtilizadorId { get; set; }

    public int Pontos { get; set; }

    public virtual List<Favorito> Favoritos { get; set; } = new();

    [JsonIgnore]
    public virtual ICollection<RegistosPreco> RegistosPrecos { get; set; } = new List<RegistosPreco>();

    [JsonIgnore]
    public virtual ICollection<Mensagem> MensagensEnviadas { get; set; } = new List<Mensagem>();

    [JsonIgnore]
    public virtual ICollection<Mensagem> MensagensRecebidas { get; set; } = new List<Mensagem>();

    public virtual TipoUtilizador? TipoUtilizador { get; set; }
}
