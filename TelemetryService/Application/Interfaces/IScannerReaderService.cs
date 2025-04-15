using TelemetryService.Domain.Models;
using TelemetryService.Infrastructure.Scanner;

namespace TelemetryService.Application.Interfaces
{
    public interface IScannerReaderService
    {
        ScannerConnectionResult ConnectReader(ScannerConfig scannerConfig, string IpAddressHost, int PortHost);
        Task StartReadingAsync(string scannerCode, CancellationToken cancellationToken);
        void ProcessScannerEmitter(Emitter emitter);
        ScannerConnectionResult DisconnectReader(string scannerCode);
    }
}