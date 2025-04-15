namespace GeoStream.Api.Application.Dtos.Location
{
    public class CreateLocationDto
    {
        public int? CountryId { get; set; }
        public int? RegionId { get; set; }
        public int? DistrictId { get; set; }
        public int StatusId { get; set; }
    }
}
