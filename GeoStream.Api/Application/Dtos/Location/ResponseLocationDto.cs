namespace GeoStream.Api.Application.Dtos.Location
{
    public class ResponseLocationDto
    {
        public int Id { get; set; }
        public int? CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public int? RegionId { get; set; }
        public string RegionName { get; set; } = string.Empty;
        public int? DistrictId { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public int StatusId { get; set; }
    }
}
