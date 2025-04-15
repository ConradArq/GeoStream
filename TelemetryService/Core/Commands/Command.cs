using MediatR;

namespace TelemetryService.Core.Commands
{
    public abstract class Command : IRequest<bool>
    {
        public DateTime ReadTimestamp { get; protected set; }

        protected Command()
        {
            ReadTimestamp = DateTime.Now;
        }
    }
}
