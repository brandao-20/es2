using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ES2_TP_ComparadorPrecos_WebAPI.Entities;

public partial class Localizacao
{
    public int LocalizacaoId { get; set; }

    public string? Cidade { get; set; }

    public string? Pais { get; set; }

    public string? CodigoPostal { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? GoogleMapsUrl { get; set; }

    [JsonIgnore]
    public virtual ICollection<Loja> Lojas { get; set; } = new List<Loja>();
}
