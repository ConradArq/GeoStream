using MessageProcessorService.Core.Events;

namespace MessageProcessorService.Domain.Events
{
    /// <summary>
    /// Event published when a Emitter has been read from the scanner.
    /// </summary>
    public class EmitterReadEvent : Event
    {
        public string ScannerCode { get; set; }
        public string EmitterCode { get; set; }

        public EmitterReadEvent(string scannerCode, string emitterCode)
        {
            ScannerCode = scannerCode;
            EmitterCode = emitterCode;
        }
    }
}
