using MediatR;
using TelemetryService.Application.Commands;
using TelemetryService.Core.Interfaces;
using TelemetryService.Domain.Events;

namespace TelemetryService.Application.CommandHandlers
{
    public class EmitterReadCommandHandler : IRequestHandler<EmitterReadCommand, bool>
    {
        private readonly IEventBus _eventBus;

        public EmitterReadCommandHandler(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public Task<bool> Handle(EmitterReadCommand request, CancellationToken cancellationToken)
        {            
            _eventBus.Publish(new EmitterReadEvent(request.ScannerCode, request.Code));

            return Task.FromResult(true);
        }
    }
}
