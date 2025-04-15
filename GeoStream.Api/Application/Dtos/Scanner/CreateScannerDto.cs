using System.ComponentModel;

namespace GeoStream.Api.Application.Dtos.Scanner
{
    public class CreateScannerDto
    {
        public string Code { get; set; } = string.Empty;
        public float LaneDirectionDegrees { get; set; }
        public string LaneDestination { get; set; } = string.Empty;
        public bool IsConnected { get; set; } = true;
        public int? EmitterReadingIntervalInMinutes { get; set; }

        public int HubId { get; set; }

        [DefaultValue((int)Domain.Enums.Status.Active)]
        public int StatusId { get; set; } = (int)Domain.Enums.Status.Active;
    }
}