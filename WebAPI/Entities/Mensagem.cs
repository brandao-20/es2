namespace WebAPI.Entities
{
    public class Mensagem
    {
        public int MensagemId { get; set; }
        public int RemetenteId { get; set; } // Quem envia (admin ou usuário)
        public Utilizador? Remetente { get; set; }
        public int DestinatarioId { get; set; } // Quem recebe (admin ou usuário)
        public Utilizador? Destinatario { get; set; }
        public string? Conteudo { get; set; }
        public DateTime DataEnvio { get; set; }
    }
}
