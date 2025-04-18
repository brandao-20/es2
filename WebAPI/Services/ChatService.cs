using WebAPI.Entities;
using WebAPI.Observers;
using WebAPI.Repositories;

namespace WebAPI.Services
{
    public class ChatService
    {
        private readonly List<IMessageObserver> _observers = new List<IMessageObserver>();
        private readonly IMensagemRepository _mensagemRepository;
        private readonly IMessageObserverRegistry _observerRegistry;

        public ChatService(IMensagemRepository mensagemRepository, IMessageObserverRegistry observerRegistry)
        {
            _mensagemRepository = mensagemRepository;
            _observerRegistry = observerRegistry;
            _observerRegistry.RegisterObservers(this);
        }

        public void RegisterObserver(IMessageObserver observer)
        {
            _observers.Add(observer);
        }

        public void RemoveObserver(IMessageObserver observer)
        {
            _observers.Remove(observer);
        }

        public async Task SendMessageAsync(Mensagem mensagem)
        {
            // Salva a mensagem no banco de dados
            await _mensagemRepository.AddAsync(mensagem);

            // Notifica todos os observadores
            foreach (var observer in _observers)
            {
                await observer.NotifyAsync(mensagem);
            }
        }
    }
}
