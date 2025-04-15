using GeoStream.RabbitMQ.Contracts.Events;

namespace GeoStream.RabbitMQ.Contracts.Bus
{
    public interface IEventBus
    {
        void Suscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>;
    }
}
