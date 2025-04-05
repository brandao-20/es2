namespace WebApp.Models
{
    public class Mensagem
    {
        public int MensagemId { get; set; }
        public int RemetenteId { get; set; } 
        public Utilizador? Remetente { get; set; }
        public int DestinatarioId { get; set; } 
        public Utilizador? Destinatario { get; set; }
        public string? Conteudo { get; set; }
        public DateTime DataEnvio { get; set; }
    }
}
