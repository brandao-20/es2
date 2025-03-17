using System;
using System.Collections.Generic;

namespace ES2_TP_ComparadorPrecos_WebAPI.Entities;

public partial class RegistosPreco
{
    public int RegistoPrecoId { get; set; }

    public decimal Preco { get; set; }

    public DateTime DataRegisto { get; set; }

    public decimal Credibilidade { get; set; }

    public int TipoAcaoId { get; set; }

    public int ProdutoId { get; set; }

    public int LojaId { get; set; }

    public int UtilizadorId { get; set; }

    public virtual Loja? Loja { get; set; } = null!;

    public virtual Produto? Produto { get; set; } = null!;

    public virtual TipoAcao? TipoAcao { get; set; } = null!;

    public virtual Utilizador? Utilizador { get; set; } = null!;
}
