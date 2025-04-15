using System.ComponentModel.DataAnnotations.Schema;
using GeoStream.Api.Domain.Attributes;

namespace GeoStream.Api.Domain.Models.Entities
{
    [Discoverable]
    public class Hub : BaseDomainModel
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Address { get; set; } = null!;
        [Column(TypeName = "decimal(15, 6)")]
        public decimal Latitude { get; set; }
        [Column(TypeName = "decimal(15, 6)")]
        public decimal Longitude { get; set; }
        public int LocationId { get; set; }
        public virtual Location Location { get; set; } = null!;
        public virtual List<Scanner> Scanners { get; set; } = null!;
        public virtual List<RouteHub> RouteHubs { get; set; } = null!;
    }
}