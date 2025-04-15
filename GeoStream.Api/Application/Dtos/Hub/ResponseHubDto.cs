
using GeoStream.Api.Application.Dtos.Location;
using GeoStream.Api.Application.Dtos.RouteHub;
using GeoStream.Api.Application.Dtos.Scanner;

namespace GeoStream.Api.Application.Dtos.Hub
{
    public class ResponseHubDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int LocationId { get; set; }
        public int StatusId { get; set; }

        public ResponseLocationDto Location { get; set; } = new();
        public List<ResponseScannerDto> Scanners { get; set; } = new List<ResponseScannerDto>();
        public List<ResponseRouteHubDto> RouteHubs { get; set; } = new List<ResponseRouteHubDto>();
    }
}
