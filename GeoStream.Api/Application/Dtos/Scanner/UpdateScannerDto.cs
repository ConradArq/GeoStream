using System.ComponentModel;

namespace GeoStream.Api.Application.Dtos.Scanner
{
    public class UpdateScannerDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public float? LaneDirectionDegrees { get; set; }
        public string? LaneDestination { get; set; }
        public bool? IsConnected { get; set; }
        public int? EmitterReadingIntervalInMinutes { get; set; }

        public int? HubId { get; set; }

        [DefaultValue((int)Domain.Enums.Status.Active)]
        public int? StatusId { get; set; } = (int)Domain.Enums.Status.Active;
    }
}