using System.ComponentModel.DataAnnotations;

namespace WebAPI.Entities
{
    public class Comentario
    {
        public int ComentarioId { get; set; }
        public int RegistoPrecoId { get; set; }
        public int UtilizadorId { get; set; }
        public string Conteudo { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }

        // Campos de navegação não devem ser obrigatórios
        public RegistosPreco? RegistoPreco { get; set; }
        public Utilizador? Utilizador { get; set; }
    }
}
