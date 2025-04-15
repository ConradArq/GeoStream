using System.ComponentModel;

namespace GeoStream.Api.Application.Dtos.Scanner
{
    public class SearchScannerDto : QueryRequestDto
    {
        public string? Code { get; set; }
        public float? LaneDirectionDegrees { get; set; }
        public string? LaneDestination { get; set; }
        public bool? IsConnected { get; set; }
        public int? EmitterReadingIntervalInMinutes { get; set; }
        public int? HubId { get; set; }
    }
}
