using System.ComponentModel.DataAnnotations;

namespace GeoStream.Dtos.Configuration
{
    public class LocationDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Field is required")]
        public int? CountryId { get; set; }

        public string CountryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field is required")]
        public int? RegionId { get; set; }

        public string RegionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field is required")]
        public int? DistrictId { get; set; }

        public string DistrictName { get; set; } = string.Empty;
    }
}
