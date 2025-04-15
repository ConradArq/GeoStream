namespace GeoStream.Api.Domain.Models.Entities
{
    public class Country : BaseDomainModel
    {
        public string Name { get; set; } = null!;
        public virtual List<Region> Regions { get; set; } = null!;
    }
}
