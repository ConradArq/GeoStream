using System.ComponentModel;
using GeoStream.Api.Application.Dtos.Location;
using GeoStream.Api.Application.Dtos.RouteHub;
using GeoStream.Api.Application.Dtos.Scanner;

namespace GeoStream.Api.Application.Dtos.Hub
{
    public class UpdateHubDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? LocationId { get; set; }

        [DefaultValue((int)Domain.Enums.Status.Active)]
        public int? StatusId { get; set; }

        public UpdateLocationDto Location { get; set; } = new UpdateLocationDto();
        public List<UpdateScannerDto> Scanners { get; set; } = new List<UpdateScannerDto>();
        public List<UpdateRouteHubDto> RouteHubs { get; set; } = new List<UpdateRouteHubDto>();
    }
}