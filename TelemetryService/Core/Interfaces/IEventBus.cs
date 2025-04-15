using TelemetryService.Core.Commands;
using TelemetryService.Core.Events;

namespace TelemetryService.Core.Interfaces
{
    public interface IEventBus
    {
        Task SendCommand<T>(T command) where T : Command;
        void Publish<T>(T @event) where T : Event;
    }
}
