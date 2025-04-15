using Microsoft.Extensions.Logging;
using GeoStream.RabbitMQ.Contracts.Events;

namespace GeoStream.RabbitMQ.Contracts.Bus
{
    public interface IEventHandler<in TEvent> : IEventHandler
            where TEvent : Event
    {
        Task Handle(TEvent @event);
    }

    public interface IEventHandler
    {

    }
}
