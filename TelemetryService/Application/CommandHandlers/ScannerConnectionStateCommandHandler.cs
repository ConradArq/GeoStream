using MediatR;
using TelemetryService.Application.Commands;
using TelemetryService.Core.Interfaces;
using TelemetryService.Domain.Events;

namespace TelemetryService.Application.CommandHandlers
{
    public class ScannerConnectionStateCommandHandler : IRequestHandler<ScannerConnectionStateCommand, bool>
    {
        private readonly IEventBus _eventBus;

        public ScannerConnectionStateCommandHandler(IEventBus bus)
        {
            _eventBus = bus;
        }

        public Task<bool> Handle(ScannerConnectionStateCommand request, CancellationToken cancellationToken)
        {            
            _eventBus.Publish(new ScannerConnectionStateEvent(request.ScannerCode, request.IsConnected));

            return Task.FromResult(true);
        }
    }
}
