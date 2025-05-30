namespace WebApp.Models
{
    public class Comentario
    {
        public int ComentarioId { get; set; }
        public int RegistoPrecoId { get; set; }
        public int UtilizadorId { get; set; }
        public string Conteudo { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public Utilizador? Utilizador { get; set; }
        public RegistosPreco? RegistoPreco { get; set; }
    }
}