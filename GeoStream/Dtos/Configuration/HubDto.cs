using GeoStream.Attributes;
using System.ComponentModel.DataAnnotations;

namespace GeoStream.Dtos.Configuration
{
    public class HubDto : BaseDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Field is required")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field is required")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        [NotEqualToZeroAttribute(ErrorMessage = "Field is required")]
        public decimal Latitude { get; set; }

        [NotEqualToZeroAttribute(ErrorMessage = "Field is required")]
        public decimal Longitude { get; set; }

        public int LocationId { get; set; }
        public LocationDto Location { get; set; } = new();
        public List<ScannerDto> Scanners { get; set; } = new List<ScannerDto>();
        public List<RouteHubDto> RouteHubs { get; set; } = new List<RouteHubDto>();
    }
}
