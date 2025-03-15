namespace ES2_TP_ComparadorPrecos_BACKEND.models;

public class RegistoPreco
{
    public int RegistoPrecoId { get; set; }
    public decimal Preco { get; set; }
    public DateTime DataRegisto { get; set; }
    public double Credibilidade { get; set; }

    // Exemplo de enum
    public TipoAcao TipoAcao { get; set; }

    // FKs
    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    public int LojaId { get; set; }
    public Loja? Loja { get; set; }

    public int UtilizadorId { get; set; }
    public Utilizador? Utilizador { get; set; }
}

// Exemplo do enum
public enum TipoAcao
{
    CONFIRMACAO,
    ATUALIZACAO,
    NOVO_REGISTO
}