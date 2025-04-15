namespace GeoStream.Api.Domain.Models.Entities
{
    public class Location : BaseDomainModel
    {
        public int? CountryId { get; set; }
        public int? RegionId { get; set; }
        public int? DistrictId { get; set; }
        public virtual Country Country { get; set; } = null!;
        public virtual Region Region { get; set; } = null!;
        public virtual District District { get; set; } = null!;
        public virtual List<Hub> Hubs { get; set; } = null!;
    }
}
