using GeoStream.Api.Domain.Attributes;

namespace GeoStream.Api.Domain.Models.Entities
{
    [Discoverable]
    public class SpecialAccess : BaseDomainModel
    {
        public int AssetId { get; set; }
        public int RouteId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual Asset Asset { get; set; } = null!;
        public virtual Route Route { get; set; } = null!;
    }
}
