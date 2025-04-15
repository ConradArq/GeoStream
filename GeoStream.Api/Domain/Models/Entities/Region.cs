namespace GeoStream.Api.Domain.Models.Entities
{
    public class Region : BaseDomainModel
    {
        public string Name { get; set; } = null!;
        public int CountryId { get; set; }
        public virtual Country Country { get; set; } = null!;
        public virtual List<District> Districts { get; set; } = null!;
    }
}
