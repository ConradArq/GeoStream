using GeoStream.Api.Domain.Attributes;
using GeoStream.Api.Domain.Enums;

namespace GeoStream.Api.Domain.Models.Entities
{
    [Discoverable]
    public class Route : BaseDomainModel
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public byte[] KmzFile { get; set; } = null!;
        public string GeoJson { get; set; } = null!;
        public string Color { get; set; } = null!;
        public TypeOfRoute? TypeOfRoute { get; set; }
        public virtual List<Asset> Assets { get; set; } = null!;
        public virtual List<RouteHub> RouteHubs { get; set; } = null!;
    }
}

