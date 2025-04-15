using GeoStream.Api.Domain.Attributes;

namespace GeoStream.Api.Domain.Models.Entities
{
    [Discoverable]
    public class Scanner : BaseDomainModel
    {
        public string Code { get; set; } = null!;
        public float LaneDirectionDegrees { get; set; }
        public string LaneDestination { get; set; } = null!;
        public bool IsConnected { get; set; } = true;
        public int? EmitterReadingIntervalInMinutes { get; set; }
        public int HubId { get; set; }
        public virtual Hub Hub { get; set; } = null!;
    }
}