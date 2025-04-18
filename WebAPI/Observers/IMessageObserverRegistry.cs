using WebAPI.Services;

namespace WebAPI.Observers
{
    public interface IMessageObserverRegistry
    {
        void RegisterObservers(ChatService chatService);
    }
}