namespace WebAPI.DTOs
{
    public class PrecoComparacaoDTO
    {
        public string NomeProduto { get; set; }
        public List<LojaDTO> LojasDisponiveis { get; set; }
        public List<PrecoAtualDTO> PrecosAtuais { get; set; }
        public List<HistoricoPrecoDTO> HistoricoPrecos { get; set; }
    }

    public class LojaDTO
    {
        public int LojaId { get; set; }
        public string Nome { get; set; }
    }

    public class PrecoAtualDTO
    {
        public string NomeLoja { get; set; }
        public decimal Preco { get; set; }
        public DateTime DataRegisto { get; set; }
    }

    public class HistoricoPrecoDTO
    {
        public string NomeLoja { get; set; }
        public decimal Preco { get; set; }
        public DateTime DataRegisto { get; set; }
    }
}
