using System;
using System.Collections.Generic;

namespace WebAPI.Entities;

public partial class TipoAcao
{
    public int TipoAcaoId { get; set; }

    public string Tipo { get; set; } = null!;

    public virtual ICollection<RegistosPreco> RegistosPrecos { get; set; } = new List<RegistosPreco>();
}
