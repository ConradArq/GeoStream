using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using GeoStream.Api.Infrastructure.Interfaces.Services;

namespace GeoStream.Api.Infrastructure.Logging
{
    public class ApiLoggerProvider : ILoggerProvider
    {
        private readonly ILogQueueService _logQueueService;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public ApiLoggerProvider(ILogQueueService logQueueService, IHttpContextAccessor? httpContextAccessor)
        {
            _logQueueService = logQueueService;
            _httpContextAccessor = httpContextAccessor;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new ApiLogger(categoryName, _logQueueService, _httpContextAccessor);
        }

        public void Dispose()
        {
        }
    }
}
