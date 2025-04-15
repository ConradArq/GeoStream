using TelemetryService.Application.Commands;
using TelemetryService.Application.Interfaces;
using TelemetryService.Core.Interfaces;
using TelemetryService.Domain.Models;
using TelemetryService.Infrastructure.Scanner;

namespace TelemetryService.Application.Services
{
    public class ScannerService : IScannerReaderService
    {
        private readonly IEventBus _eventBus;

        public ScannerService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public ScannerConnectionResult ConnectReader(ScannerConfig scannerConfig, string IpAddressHost, int PortHost)
        {
            // Implement actual logic to connect to scanner
            return new ScannerConnectionResult() { Connected = true };
        }

        public async Task StartReadingAsync(string scannerCode, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Implement actual logic to start reading from scanner

                ProcessScannerEmitter(new Emitter() { ScannerCode = scannerCode, Code = "123" });

                try
                {
                    await Task.Delay(5000, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        public void ProcessScannerEmitter(Emitter emitter)
        {
            var command = new EmitterReadCommand(emitter.Code, emitter.ScannerCode);
            _eventBus.SendCommand(command);
        }

        public ScannerConnectionResult DisconnectReader(string scannerCode)
        {
            // Implement actual logic to disconnect from scanner closing connections and disposing unmanaged resources
            return new ScannerConnectionResult() { Connected = false };
        }
    }
}