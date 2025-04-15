using GeoStream.Api.Domain.Attributes;

namespace GeoStream.Api.Domain.Models.Entities
{
    [Discoverable]
    public class Asset: BaseDomainModel
    {
        public string Code { get; set; } = null!;      
        public string OwnerDocumentNumber { get; set; } = null!;       
        public int RouteId { get; set; }
        public virtual Route Route { get; set; } = null!;
        public virtual List<SpecialAccess> SpecialAccesss { get; set; } = null!;
        public virtual List<AssetEmitter> AssetEmitters { get; set; } = null!;
    }
}
