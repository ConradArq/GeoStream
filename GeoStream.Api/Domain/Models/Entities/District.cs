namespace GeoStream.Api.Domain.Models.Entities
{
    public class District : BaseDomainModel
    {
        public string Name { get; set; } = null!;
        public int RegionId { get; set; }
        public virtual Region Region { get; set; } = null!;
    }
}
