using GeoStream.RabbitMQ.Contracts.Events;

namespace GeoStream.RabbitMQ.Events
{
    public class ScannerConnectionStateEvent : Event
    {
        public string ScannerCode { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
    }
}
