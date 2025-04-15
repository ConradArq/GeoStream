using TelemetryService.Core.Commands;

namespace TelemetryService.Application.Commands
{
    public class ScannerConnectionStateCommand : Command
    {
        public string ScannerCode { get; set; }
        public bool IsConnected { get; set; }

        public ScannerConnectionStateCommand(string scannerCode, bool isConnected) 
        {
            ScannerCode = scannerCode;
            IsConnected = isConnected;
        }   
    }
}
