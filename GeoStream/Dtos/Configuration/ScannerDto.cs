using System.ComponentModel.DataAnnotations;

namespace GeoStream.Dtos.Configuration
{
    public class ScannerDto : BaseDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Field is required")]
        public string Code { get; set; } = string.Empty;

        // Direction in degrees indicating where the scanner is pointing (range: 0 to 360)
        [Range(0, 360, ErrorMessage = "The value must be between 0 and 360")]
        public float LaneDirectionDegrees { get; set; }

        // Destination location of the lane where the scanner is located
        [Required(ErrorMessage = "Field is required")]
        public string LaneDestination { get; set; } = string.Empty;

        public bool IsConnected { get; set; } = true;

        // Interval in minutes to wait before storing the same Emitter in MongoDB
        public int? EmitterReadingIntervalInMinutes { get; set; }
        public int HubId { get; set; }
        public string HubName { get; set; } = string.Empty;
        public string HubCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
    }
}
