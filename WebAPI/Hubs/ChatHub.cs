using Microsoft.AspNetCore.Authorization;
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
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            await base.OnConnectedAsync();
        }

        public async Task SendMessage(int destinatarioId, string message)
        {
            var remetenteId = int.Parse(Context.User?.FindFirst("utilizadorId")?.Value ?? "0");
            var remetente = await _utilizadorRepository.GetByIdAsync(remetenteId);
            var mensagem = new Mensagem
            {
                RemetenteId = remetenteId,
                DestinatarioId = destinatarioId,
                Conteudo = message,
                DataEnvio = DateTime.UtcNow
            };

            await _mensagemRepository.AddAsync(mensagem);
            await Clients.Group($"user-{destinatarioId}").SendAsync("ReceiveMessage", remetente.Username, message, mensagem.DataEnvio.ToString("dd/MM/yyyy HH:mm"));
            await Clients.Caller.SendAsync("ReceiveMessage", remetente.Username, message, mensagem.DataEnvio.ToString("dd/MM/yyyy HH:mm"));
        }

        public async Task GetMessageHistory(int userId)
        {
            var mensagens = await _mensagemRepository.GetByUserIdAsync(userId);
            foreach (var msg in mensagens)
            {
                var sender = msg.Remetente?.Username ?? "Desconhecido";
                await Clients.Caller.SendAsync("ReceiveMessage", sender, msg.Conteudo, msg.DataEnvio.ToString("dd/MM/yyyy HH:mm"));
            }
        }
    }
}
