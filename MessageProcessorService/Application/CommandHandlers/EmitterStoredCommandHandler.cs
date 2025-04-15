using MediatR;
using MessageProcessorService.Application.Commands;
using MessageProcessorService.Core.Interfaces;
using MessageProcessorService.Domain.Events;

namespace MessageProcessorService.Application.CommandHandlers
{
    public class EmitterStoredCommandHandler : IRequestHandler<EmitterStoredCommand, bool>
    {
        private readonly IEventBus _bus;

        public EmitterStoredCommandHandler(IEventBus bus)
        {
            _bus = bus;
        }

        public Task<bool> Handle(EmitterStoredCommand request, CancellationToken cancellationToken)
        {
            EmitterStoredEvent emitterStoredEvent = new EmitterStoredEvent(
                request.ScannerCode,
                request.Code,
                request.HubCode,
                request.Latitude,
                request.Longitude,
                request.LaneDirectionDegrees,
                request.Destination,
                request.AssetCode,
                request.IncidentTypes
            );

            _bus.Publish(emitterStoredEvent);

            return Task.FromResult(true);
        }
    }
}
