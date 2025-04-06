using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Entities;
using WebAPI.Repositories;

namespace WebAPI.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMensagemRepository _mensagemRepository;
        private readonly IUtilizadorRepository _utilizadorRepository;

        public ChatHub(IMensagemRepository mensagemRepository, IUtilizadorRepository utilizadorRepository)
        {
            _mensagemRepository = mensagemRepository;
            _utilizadorRepository = utilizadorRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = int.Parse(Context.User?.FindFirst("utilizadorId")?.Value ?? "0");
            if (userId != 0)
            {
                Console.WriteLine($"[DEBUG] Utilizador {userId} conectado ao SignalR via {Context.Features.Get<IHttpTransportFeature>()?.TransportType}");
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            }
            else
            {
                Console.WriteLine("[ERROR] Utilizador não identificado ao conectar ao SignalR.");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = int.Parse(Context.User?.FindFirst("utilizadorId")?.Value ?? "0");
            Console.WriteLine($"[DEBUG] Utilizador {userId} desconectado do SignalR. Erro: {exception?.Message}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(int destinatarioId, string message)
        {
            var remetenteId = int.Parse(Context.User?.FindFirst("utilizadorId")?.Value ?? "0");
            var remetente = await _utilizadorRepository.GetByIdAsync(remetenteId);
            if (remetente == null)
            {
                Console.WriteLine($"[ERROR] Remetente {remetenteId} não encontrado.");
                return;
            }

            var mensagem = new Mensagem
            {
                RemetenteId = remetenteId,
                DestinatarioId = destinatarioId,
                Conteudo = message,
                DataEnvio = DateTime.UtcNow
            };

            await _mensagemRepository.AddAsync(mensagem);
            Console.WriteLine($"[DEBUG] Mensagem enviada de {remetenteId} para {destinatarioId}: {message}");
            await Clients.Group($"user-{destinatarioId}").SendAsync("ReceiveMessage", remetente.Username, remetenteId, destinatarioId, message, mensagem.DataEnvio);
            await Clients.Caller.SendAsync("ReceiveMessage", remetente.Username, remetenteId, destinatarioId, message, mensagem.DataEnvio);
        }
    }
}
