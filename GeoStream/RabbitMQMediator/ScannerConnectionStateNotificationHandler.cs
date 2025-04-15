using MediatR;
using GeoStream.UIEventsMediator;

namespace GeoStream.RabbitMQMediator
{
    public class ScannerConnectionStateNotificationHandler : INotificationHandler<ScannerConnectionStateNotification>
    {
        private readonly IEventAggregator _eventaggregator;

        public ScannerConnectionStateNotificationHandler(IEventAggregator eventaggregator)
        {
            _eventaggregator = eventaggregator;
        }

        public Task Handle(ScannerConnectionStateNotification notification, CancellationToken cancellationToken)
        {
            _eventaggregator.Publish(notification);
            return Task.CompletedTask;
        }
    }
}
