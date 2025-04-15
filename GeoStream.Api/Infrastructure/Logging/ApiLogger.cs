using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using GeoStream.Api.Infrastructure.Interfaces.Logging;
using GeoStream.Api.Infrastructure.Interfaces.Services;
using GeoStream.Api.Infrastructure.Logging.Models;
using GeoStream.Api.Infrastructure.Logging.Models.Enums;
using System.Security.Claims;
using GeoStream.Api.Extensions;

namespace GeoStream.Api.Infrastructure.Logging
{
    /// <summary>
    /// This class is created by the <see cref="ApiLoggerProvider"/> when resolved as an <see cref="ILogger"/>, 
    /// and is used alongside other loggers implementing <see cref="ILogger"/> created by other logging providers 
    /// configured in .NET, such as the Console Logger Provider.
    /// 
    /// When resolved as an <see cref="IApiLogger"/>, this class is instantiated using the callback function 
    /// provided during its registration in the DI container.
    /// 
    /// - **ILogger**: Inject this interface if you require standard logging methods and the extension methods provided by 
    /// the <see cref="LoggerExtensions"/> class.
    /// 
    /// - **IApiLogger**: Adds custom logging methods that extend beyond the standard <see cref="ILogger"/> functionality. 
    /// Inject this interface if you require these custom methods.
    /// </summary>


    public class ApiLogger : ILogger, IApiLogger
    {
        private static readonly AsyncLocal<bool> _isLogging = new();

        private readonly string _categoryName;
        private readonly ILogQueueService _logQueueService;
        private readonly IHttpContextAccessor? _httpContextAccessor;


        public ApiLogger(string categoryName, ILogQueueService logQueueService, IHttpContextAccessor? httpContextAccessor)
        {
            _categoryName = categoryName;
            _logQueueService = logQueueService;
            _httpContextAccessor = httpContextAccessor;
        }

        // Hidden from the main public API by explicitly implementing the ILogger interface, as scopes are not used in this implementation.
        IDisposable? ILogger.BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel)
        {
            // Prevent feedback loop
            if (_isLogging.Value) return false;

            return logLevel >= LogLevel.Information;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            // Prevent recursive logging
            _isLogging.Value = true;
            try
            {
                var message = formatter(state, exception);
                object? logEntry = logLevel switch
                {
                    LogLevel.Trace or LogLevel.Debug or LogLevel.Information or LogLevel.Warning
                    => new AuditLog
                    {
                        UserId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System",
                        EventType = EventType.Others,
                        Message = $"[{_categoryName}] {logLevel}: {message}"
                    },
                    LogLevel.Error or LogLevel.Critical => new ErrorLog
                    {
                        UserId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System",
                        ExceptionMessage = $"[{_categoryName}] {logLevel}: {message ?? exception?.Message ?? string.Empty}",
                        ExceptionType = exception?.GetType().Name ?? string.Empty,
                        ErrorStackTrace = exception?.StackTrace ?? string.Empty,
                        ErrorDate = DateTime.Now.InTimeZone()
                    },
                    _ => null
                };

                if (logEntry != null)
                {
                    _logQueueService.EnqueueLog(logEntry);
                }
            }
            finally
            {
                _isLogging.Value = false;
            }
        }

        public void LogInfo(AuditLog logEntry)
        {
            logEntry.UserId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
            logEntry.RequestUrl = _httpContextAccessor?.HttpContext?.Request.GetDisplayUrl();
            logEntry.EventType = logEntry.EventType == default ? EventType.Others : logEntry.EventType;

            _logQueueService.EnqueueLog(logEntry);
        }

        public void LogError(ErrorLog logEntry)
        {
            logEntry.UserId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
            logEntry.ErrorDate = DateTime.Now.InTimeZone();

            _logQueueService.EnqueueLog(logEntry);
        }
    }
}
