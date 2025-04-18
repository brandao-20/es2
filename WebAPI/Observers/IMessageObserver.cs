using WebAPI.Entities;

namespace WebAPI.Observers
{
    public interface IMessageObserver
    {
        Task NotifyAsync(Mensagem mensagem);
    }
}
