using GeoStream.Api.Domain.Attributes;

namespace GeoStream.Api.Domain.Models.Entities
{
    [Discoverable]
    public class AssetEmitter : BaseDomainModel
    {
        public string EmitterCode { get; set; } = null!;

        public int AssetId { get; set; }
        public virtual Asset Asset { get; set; } = null!;
    }
}
