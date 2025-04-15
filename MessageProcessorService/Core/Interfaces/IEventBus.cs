using MessageProcessorService.Core.Commands;
using MessageProcessorService.Core.Events;

namespace MessageProcessorService.Core.Interfaces
{
    public interface IEventBus
    {
        void Subscribe<T, TH>() where T : Event where TH : IEventHandler<T>;
        Task SendCommand<T>(T command) where T : Command;
        void Publish<T>(T @event) where T : Event;
    }
}
