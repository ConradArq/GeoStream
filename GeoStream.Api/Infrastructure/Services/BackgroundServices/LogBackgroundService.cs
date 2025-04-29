using GeoStream.Api.Infrastructure.Logging.Models;
using GeoStream.Api.Infrastructure.Interfaces.Providers;
using GeoStream.Api.Infrastructure.Interfaces.Services;

namespace GeoStream.Api.Infrastructure.Services.BackgroundServices
{
    public class LogBackgroundService : BackgroundService
    {
        private readonly ILogQueueService _logQueueService;
        private readonly IHttpService _httpService;
        private readonly ILogger<LogBackgroundService> _logger;
        private readonly IJwtTokenProvider _jwtTokenProvider;
        private readonly string _loggingApiBaseUrl;
        private readonly string _createAuditLogEndpoint;
        private readonly string _createErrorLogEndpoint;
        private readonly int _maxRetryAttempts;
        private readonly TimeSpan _retryInterval;
        private readonly SemaphoreSlim _semaphore;

        public LogBackgroundService(
            ILogQueueService logQueueService,
            IHttpService httpService,
            ILogger<LogBackgroundService> logger,
            IConfiguration configuration,
            IJwtTokenProvider jwtTokenProvider)
        {
            _logQueueService = logQueueService;
            _httpService = httpService;
            _logger = logger;
            _jwtTokenProvider = jwtTokenProvider;

            _loggingApiBaseUrl = configuration["ExternalApis:Logging:BaseUrl"] ?? throw new ArgumentNullException("Logging API base URL configuration is missing.");
            _createAuditLogEndpoint = configuration["ExternalApis:Logging:Endpoints:CreateAuditLog"] ?? throw new ArgumentNullException("Logging API CreateAuditLog URL configuration is missing.");
            _createErrorLogEndpoint = configuration["ExternalApis:Logging:Endpoints:CreateErrorLog"] ?? throw new ArgumentNullException("Logging API CreateErrorLog URL configuration is missing.");

            if (int.TryParse(configuration["ExternalApis:Logging:MaxRetryAttempts"], out int parsedMaxRetryAttempts))
            {
                _maxRetryAttempts = parsedMaxRetryAttempts;
            }

            if (double.TryParse(configuration["ExternalApis:Logging:RetryIntervalInMinutes"], out double parsedRetryIntervalInMinutes))
            {
                _retryInterval = TimeSpan.FromMinutes(parsedRetryIntervalInMinutes);
            }

            _semaphore = new SemaphoreSlim(5);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var logEntry = await _logQueueService.DequeueLogAsync(stoppingToken);

                if (logEntry != null)
                {
                    _ = ProcessLogInParallelAsync(logEntry, stoppingToken);
                }
            }
        }

        // Start processing log entry with bounded parallelism
        private async Task ProcessLogInParallelAsync(object logEntry, CancellationToken stoppingToken)
        {
            await _semaphore.WaitAsync(stoppingToken);
            try
            {
                await ProcessLogEntryWithRetryAsync(logEntry, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in logging background service.");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task ProcessLogEntryWithRetryAsync(object logEntry, CancellationToken stoppingToken)
        {
            string apiUrl;

            switch (logEntry)
            {
                case AuditLog:
                    apiUrl = string.Concat(_loggingApiBaseUrl, _createAuditLogEndpoint);
                    break;
                case ErrorLog:
                    apiUrl = string.Concat(_loggingApiBaseUrl, _createErrorLogEndpoint);
                    break;
                default:
                    throw new ArgumentException("Log entry is not valid.", nameof(logEntry));
            }

            int retryCount = 0;
            bool success = false;
            var authorizationToken = _jwtTokenProvider.GenerateAuthenticationToken();

            while (!success && retryCount <= _maxRetryAttempts && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _httpService.SendAsync<object>(HttpMethod.Post, apiUrl, logEntry, "application/json", new Dictionary<string, string>() { { "Authorization", string.Concat("Bearer ", authorizationToken) } });
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error sending log to API: {ex.Message}", ex);
                }
                finally
                {
                    retryCount++;
                    if (!success && retryCount <= _maxRetryAttempts)
                    {
                        await Task.Delay(_retryInterval, stoppingToken);
                    }
                }
            }
        }
    }
}