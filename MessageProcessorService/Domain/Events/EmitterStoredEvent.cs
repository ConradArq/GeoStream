using MessageProcessorService.Core.Events;
using MessageProcessorService.Domain.Enums;

namespace MessageProcessorService.Domain.Events
{
    /// <summary>
    /// Event to be published when a Emitter read from the scanner has been stored in MongoDB.
    /// </summary>
    public class EmitterStoredEvent : Event
    {
        public string ScannerCode { get; set; }
        public string EmitterCode { get; set; }
        public string HubCode { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public float LaneDirectionDegrees { get; set; }
        public string Destination { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public List<IncidentType> IncidentTypes { get; set; }

        public EmitterStoredEvent(string scannerCode, string emitterCode, string hubCode, decimal latitude, decimal longitude, float laneDirectionDegrees, string destination, string assetCode, List<IncidentType> incidentTypes)
        {
            ScannerCode = scannerCode;
            EmitterCode = emitterCode;
            HubCode = hubCode;
            Latitude = latitude;
            Longitude = longitude;
            LaneDirectionDegrees = laneDirectionDegrees;
            Destination = destination;
            AssetCode = assetCode;
            IncidentTypes = incidentTypes;
        }
    }
}
