using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Context;
using WebAPI.Entities;
using WebAPI.Repositories;
using WebAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ChatService _chatService;
        private readonly IUtilizadorRepository _utilizadorRepository;
        private readonly AppDbContext _context;

        public ChatHub(ChatService chatService, IUtilizadorRepository utilizadorRepository, AppDbContext context)
        {
            _chatService = chatService;
            _utilizadorRepository = utilizadorRepository;
            _context = context;
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

            await _chatService.SendMessageAsync(mensagem);
            Console.WriteLine($"[DEBUG] Mensagem enviada de {remetenteId} para {destinatarioId}: {message}");
        }

        public async Task NotifyPriceChange(int produtoId, decimal preco)
        {
            var favoritos = await _context.Favoritos
                .Where(f => f.ProdutoId == produtoId)
                .Select(f => f.UtilizadorId)
                .ToListAsync();
            foreach (var userId in favoritos)
            {
                await Clients.Group($"user-{userId}").SendAsync("PriceChanged", produtoId, preco);
            }
            Console.WriteLine($"[DEBUG] Notificação de mudança de preço enviada para produto {produtoId}: {preco:C}");
        }
    }
}
