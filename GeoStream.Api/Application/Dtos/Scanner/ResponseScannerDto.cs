
using System.ComponentModel;

namespace GeoStream.Api.Application.Dtos.Scanner
{
    public class ResponseScannerDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public float LaneDirectionDegrees { get; set; }
        public string LaneDestination { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
        public int? EmitterReadingIntervalInMinutes { get; set; }
        public int HubId { get; set; }
        public int StatusId { get; set; }

        public string HubName { get; set; } = string.Empty;
        public string HubCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
    }
}
