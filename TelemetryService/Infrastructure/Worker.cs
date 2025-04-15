using Microsoft.Extensions.Options;
using TelemetryService.Domain.Models;
using TelemetryService.Application.Commands;
using TelemetryService.Application.Interfaces;
using TelemetryService.Core.Interfaces;
using TelemetryService.Infrastructure.Scanner;

namespace TelemetryService.Infrastructure
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ScannerSettings _scannerSettings;
        private Task? _listeningTask;
        private CancellationTokenSource? _listeningCancellationTokenSource = null;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEventBus _eventBus;

        public Worker(ILogger<Worker> logger, IOptions<ScannerSettings> scannersSettingsOptions, IServiceScopeFactory scopeFactory, IEventBus eventBus)
        {
            _logger = logger;
            _scannerSettings = scannersSettingsOptions.Value;
            _scopeFactory = scopeFactory;
            _eventBus = eventBus;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting service at {time}.", DateTimeOffset.Now);

            _listeningCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            if (string.IsNullOrEmpty(_scannerSettings.ScannerConfig.Code) ||
                string.IsNullOrEmpty(_scannerSettings.ScannerConfig.Connection.IpAddressReader) ||
                _scannerSettings.ScannerConfig.Connection.PortReader == 0 ||
                string.IsNullOrEmpty(_scannerSettings.HostConnection.IpAddressHost) ||
                _scannerSettings.HostConnection.PortHost == 0)
            {
                _logger.LogError($"Not all configuration parameters were found for scanner '{_scannerSettings.ScannerConfig.Code}'.");
                return;
            }

            // Fire-and-forget task to listen to the scanner in the background
            _listeningTask = ListenToScannerAsync(_listeningCancellationTokenSource.Token);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(10000, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("The service is stopping...");
            _listeningCancellationTokenSource?.Cancel();

            try
            {
                if (_listeningTask != null)
                {
                    await _listeningTask;
                    await _eventBus.SendCommand(new ScannerConnectionStateCommand(_scannerSettings.ScannerConfig.Code, false));
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("The scanner reading was cancelled.");
            }
            finally
            {
                if (_listeningCancellationTokenSource != null)
                {
                    _listeningCancellationTokenSource.Dispose();
                    _listeningCancellationTokenSource = null;
                }
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var scannerReaderService = scope.ServiceProvider.GetRequiredService<IScannerReaderService>();

                try
                {
                    scannerReaderService.DisconnectReader(_scannerSettings.ScannerConfig.Code);
                    _logger.LogInformation($"Scanner '{_scannerSettings.ScannerConfig.Code}' disconnected successfully.");

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error while disconnecting scanner '{_scannerSettings.ScannerConfig.Code}'.");
                }
            }

            await base.StopAsync(cancellationToken);
        }

        private async Task ListenToScannerAsync(CancellationToken cancellationToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var scannerReaderService = scope.ServiceProvider.GetRequiredService<IScannerReaderService>();
                int connectionAttempt = 0;
                int connectionExceptionNum = 0;

                while (!cancellationToken.IsCancellationRequested)
                {
                    ScannerConnectionResult connectionResult = new();
                    try
                    {
                        // It will only return on connection failure, exception or requested cancellation.
                        connectionResult = await ConnectAndReadFromScannerAsync();

                        // If after 5 attempts the scanner won't connect we wait 5 minutes to run the connect function for a second and last time.
                        // If connection fails again, this task completes. If connection is successfull, 'ConnectAndReadFromScannerAsync' won't
                        // return unless there's an exeption or a cancellation has been requested. In the event of an exception we disconnect
                        // the scanner and try to reconnect and read data again.
                        if (!connectionResult.Connected)
                        {
                            _logger.LogError($"The connection with scanner '{_scannerSettings.ScannerConfig.Code}' will be retried in 5 minutes.");
                            await Task.Delay(300000, cancellationToken);
                            connectionAttempt = 0;
                            await ConnectAndReadFromScannerAsync();
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        await _eventBus.SendCommand(new ScannerConnectionStateCommand(_scannerSettings.ScannerConfig.Code, false));

                        // If an exception is thrown after the scanner has being connected we disconnect and reconnect it. If disconnection fails
                        // the issue might be caused by the scanner itself. We retry disconnection for up to 10 times spacing out retries using
                        // the exponential backoff approach. After disconnection we run the loop again and try to reconnect.
                        // This is an attempt to handle edge cases like unhandled exceptions that may occur when scanner goes down or other uncommon events.
                        if (connectionResult.Connected)
                        {
                            _logger.LogError(ex, $"Error while reading from scanner '{_scannerSettings.ScannerConfig.Code}'.");

                            try
                            {
                                scannerReaderService.DisconnectReader(_scannerSettings.ScannerConfig.Code);
                                _logger.LogInformation($"Scanner '{_scannerSettings.ScannerConfig.Code}' was successfully disconnected after an exception occurred while reading from the scanner: {ex}.");
                            }
                            catch (Exception disconnectEx)
                            {
                                _logger.LogError(disconnectEx, $"Error while attempting to disconnect scanner '{_scannerSettings.ScannerConfig.Code}' after an exception occurred during scanner reading: {ex}. This error may have been caused by a problem with the scanner.");

                                bool breakMainLoop = true;
                                for (int attempt = 1; attempt <= 10; attempt++)
                                {
                                    await Task.Delay(300000 * attempt, cancellationToken);
                                    try
                                    {
                                        scannerReaderService.DisconnectReader(_scannerSettings.ScannerConfig.Code);
                                        _logger.LogInformation($"Scanner '{_scannerSettings.ScannerConfig.Code}' successfully disconnected after an exception occurred during scanner reading: {ex}.");
                                        breakMainLoop = false;
                                        break;
                                    }
                                    catch (Exception e)
                                    {
                                        _logger.LogError(e, $"Error while retrying to disconnect scanner '{_scannerSettings.ScannerConfig.Code}' after {attempt} attempt(s) following an exception during scanner reading: {ex}. This error may have been caused by a problem with the scanner.");
                                        attempt++;
                                    }
                                }
                                if (breakMainLoop)
                                {
                                    break;
                                }
                            }
                        }
                        // If an exception is thrown before the scanner has being connected we retry connection. If reconnection fails the issue
                        // might be caused by the scanner itself. We retry for up to 5 times spacing out retries by 5 minute spans. This is an
                        // attempt to handle edge cases like database failure or unhandled exceptions that may occur while connection is being established.
                        else
                        {
                            connectionExceptionNum++;
                            _logger.LogError(ex, $"Error while trying to establish a connection with scanner '{_scannerSettings.ScannerConfig.Code}'.");
                            await Task.Delay(300000, cancellationToken);

                            if (connectionExceptionNum > 5)
                            {
                                break;
                            }
                        }
                    }
                }

                // This function won't return unless connection to scanner fails
                async Task<ScannerConnectionResult> ConnectAndReadFromScannerAsync()
                {
                    int maxConnectionAttempts = 5;

                    _logger.LogInformation($"Connecting to scanner '{_scannerSettings.ScannerConfig.Code}' with IP '{_scannerSettings.ScannerConfig.Connection.IpAddressReader}' on port '{_scannerSettings.ScannerConfig.Connection.PortReader}'");
                    var connectionResult = scannerReaderService.ConnectReader(_scannerSettings.ScannerConfig, _scannerSettings.HostConnection.IpAddressHost, _scannerSettings.HostConnection.PortHost);

                    while (!connectionResult.Connected && connectionAttempt < maxConnectionAttempts && !cancellationToken.IsCancellationRequested)
                    {
                        connectionAttempt++;
                        _logger.LogError($"Failed to connect to scanner '{_scannerSettings.ScannerConfig.Code}'. Attempt {connectionAttempt} of {maxConnectionAttempts}. Retrying...");
                        await Task.Delay(1000 * connectionAttempt, cancellationToken);
                        connectionResult = scannerReaderService.ConnectReader(_scannerSettings.ScannerConfig, _scannerSettings.HostConnection.IpAddressHost, _scannerSettings.HostConnection.PortHost);
                    }

                    if (connectionResult.Connected)
                    {
                        await _eventBus.SendCommand(new ScannerConnectionStateCommand(_scannerSettings.ScannerConfig.Code, true));
                        _logger.LogInformation($"Successfully connected to scanner '{_scannerSettings.ScannerConfig.Code}'.");

                        // Start reading from the scanner; this call won't return unless an exception occurs or cancellation is requested.
                        await scannerReaderService.StartReadingAsync(_scannerSettings.ScannerConfig.Code, cancellationToken);
                    }
                    else
                    {
                        await _eventBus.SendCommand(new ScannerConnectionStateCommand(_scannerSettings.ScannerConfig.Code, false));
                        _logger.LogError($"Failed to establish connection with scanner '{_scannerSettings.ScannerConfig.Code}' after {maxConnectionAttempts} attempts.");
                    }

                    return connectionResult;
                }
            }
        }
    }
}