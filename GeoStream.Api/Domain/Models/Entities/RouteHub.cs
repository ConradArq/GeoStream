using GeoStream.Api.Domain.Attributes;

namespace GeoStream.Api.Domain.Models.Entities
{
    [Discoverable]
    public class RouteHub : BaseDomainModel
    {
        public int RouteId { get; set; }
        public int HubId { get; set; }
        public virtual Route Route { get; set; } = null!;
        public virtual Hub Hub { get; set; } = null!;
    }
}
