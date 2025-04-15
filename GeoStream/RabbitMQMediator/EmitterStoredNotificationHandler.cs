using MediatR;
using GeoStream.UIEventsMediator;

namespace GeoStream.RabbitMQMediator
{
    public class EmitterStoredNotificationHandler : INotificationHandler<EmitterStoredNotification>
    {
        private readonly IEventAggregator _eventaggregator;

        public EmitterStoredNotificationHandler(IEventAggregator eventaggregator)
        {
            _eventaggregator = eventaggregator;
        }

        public Task Handle(EmitterStoredNotification notification, CancellationToken cancellationToken)
        {
            _eventaggregator.Publish(notification);
            return Task.CompletedTask;
        }
    }

}
