using WebAPI.Entities;

namespace WebAPI.Observers
{
    public class MockEmailObserver : IMessageObserver
    {
        public async Task NotifyAsync(Mensagem mensagem)
        {
            // Simula o envio de um e-mail (lógica real seria integrada com um serviço de e-mail)
            Console.WriteLine($"[DEBUG] E-mail simulado para utilizador {mensagem.DestinatarioId}: Nova mensagem de {mensagem.RemetenteId}: {mensagem.Conteudo}");
            await Task.CompletedTask;
        }
    }
}
