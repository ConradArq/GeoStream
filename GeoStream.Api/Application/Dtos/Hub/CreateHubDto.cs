using System.ComponentModel;
using GeoStream.Api.Application.Dtos.Location;
using GeoStream.Api.Application.Dtos.RouteHub;
using GeoStream.Api.Application.Dtos.Scanner;

namespace GeoStream.Api.Application.Dtos.Hub
{
    public class CreateHubDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int LocationId { get; set; }

        [DefaultValue((int)Domain.Enums.Status.Active)]
        public int StatusId { get; set; } = (int)Domain.Enums.Status.Active;

        public CreateLocationDto Location { get; set; } = new CreateLocationDto();
        public List<CreateScannerDto> Scanners { get; set; } = new List<CreateScannerDto>();
        public List<CreateRouteHubDto> RouteHubs { get; set; } = new List<CreateRouteHubDto>();
    }
}