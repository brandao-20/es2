using Microsoft.AspNetCore.SignalR;
using WebAPI.Entities;
using WebAPI.Hubs;
using WebAPI.Observers;
using WebAPI.Repositories;

namespace WebAPI.Observers
{
    public class SignalRObserver : IMessageObserver
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IUtilizadorRepository _utilizadorRepository;

        public SignalRObserver(IHubContext<ChatHub> hubContext, IUtilizadorRepository utilizadorRepository)
        {
            _hubContext = hubContext;
            _utilizadorRepository = utilizadorRepository;
        }

        public async Task NotifyAsync(Mensagem mensagem)
        {
            var remetente = await _utilizadorRepository.GetByIdAsync(mensagem.RemetenteId);
            if (remetente == null)
            {
                Console.WriteLine($"[ERROR] Remetente {mensagem.RemetenteId} não encontrado.");
                return;
            }

            // Envia a mensagem para o destinatário e para o remetente
            await _hubContext.Clients.Group($"user-{mensagem.DestinatarioId}")
                .SendAsync("ReceiveMessage", remetente.Username, mensagem.RemetenteId, mensagem.DestinatarioId, mensagem.Conteudo, mensagem.DataEnvio);
            await _hubContext.Clients.Group($"user-{mensagem.RemetenteId}")
                .SendAsync("ReceiveMessage", remetente.Username, mensagem.RemetenteId, mensagem.DestinatarioId, mensagem.Conteudo, mensagem.DataEnvio);
        }
    }
}
