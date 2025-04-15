using GeoStream.Api.Infrastructure.Interfaces.Services;
using System.Threading.Channels;

namespace GeoStream.Api.Infrastructure.Services.Queues
{
    public class LogQueueService : ILogQueueService
    {
        private readonly Channel<object> _channel;

        public LogQueueService()
        {
            _channel = Channel.CreateUnbounded<object>();
        }

        public void EnqueueLog(object logEntry)
        {
            if (logEntry == null) throw new ArgumentNullException(nameof(logEntry));
            _channel.Writer.TryWrite(logEntry);
        }

        public async Task<object?> DequeueLogAsync(CancellationToken cancellationToken)
        {
            return await _channel.Reader.ReadAsync(cancellationToken);
        }
    }
}
