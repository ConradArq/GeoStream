using GeoStream.RabbitMQ.Contracts.Bus;
using GeoStream.RabbitMQ.Events;
using GeoStream.RabbitMQ.Models;
using MediatR;
using GeoStream.RabbitMQMediator;

namespace GeoStream.RabbitMQ.Bus
{
    public class EmitterStoredEventHandler : IEventHandler<EmitterStoredEvent>
    {
        private readonly IMediator _mediator;

        public EmitterStoredEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task Handle(EmitterStoredEvent @event)
        {
            var storedEmitterLog = new EmitterStoredLog()
            {
                ReadTimestamp = @event.ReadTimestamp,
                Emitter = @event.Emitter,
                ScannerCode = @event.ScannerCode,
                HubCode = @event.HubCode,
                Latitude = @event.Latitude,
                Longitude = @event.Longitude,
                LaneDirectionDegrees = @event.LaneDirectionDegrees,
                Destination = @event.Destination,
                AssetCode = @event.AssetCode,
                IncidentTypes = @event.IncidentTypes
            };

            await _mediator.Publish(new EmitterStoredNotification(storedEmitterLog));
        }
    }
}
