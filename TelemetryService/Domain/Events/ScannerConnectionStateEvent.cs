using TelemetryService.Core.Events;

namespace TelemetryService.Domain.Events
{
    /// <summary>
    /// Event to be publish when connection to scanner has been established or a connection failure has occurred.
    /// </summary>
    public class ScannerConnectionStateEvent : Event
    {
        public string ScannerCode { get; set; }
        public bool IsConnected { get; set; }

        public ScannerConnectionStateEvent(string scannerCode, bool isConnected)
        {
            ScannerCode = scannerCode;
            IsConnected = isConnected;
        }
    }
}
