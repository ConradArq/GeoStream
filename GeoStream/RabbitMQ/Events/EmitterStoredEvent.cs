using GeoStream.Dtos.Enums;
using GeoStream.RabbitMQ.Contracts.Events;

namespace GeoStream.RabbitMQ.Events
{
    public class EmitterStoredEvent : Event
    {
        public string Emitter { get; set; } = string.Empty;
        public string ScannerCode { get; set; } = string.Empty;
        public string HubCode { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public float LaneDirectionDegrees { get; set; }
        public string Destination { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public List<IncidentType> IncidentTypes { get; set; }
    }
}
