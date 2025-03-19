using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace WebAPI.Hubs
{
    public class ChatHub : Hub
    {
        // Envia uma mensagem para um usuário específico (baseado no ID do usuário)
        public async Task SendMessageToUser(string userId, string message)
        {
            // "UserIdentifier" deve ser configurado para que o SignalR saiba qual conexão pertence a qual usuário.
            await Clients.User(userId).SendAsync("ReceiveMessage", Context.User.Identity.Name, message);
        }

        // Envia uma mensagem para todos (opcional, se necessário)
        public async Task SendMessageToAll(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", Context.User.Identity.Name, message);
        }
    }
}
