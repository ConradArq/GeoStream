using MediatR;

namespace MessageProcessorService.Core.Commands
{
    public abstract class Command : IRequest<bool>
    {
        public DateTime ReadTimestamp { get; set; }
    }
}
