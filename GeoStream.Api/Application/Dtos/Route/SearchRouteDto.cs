using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using GeoStream.Api.Domain.Enums;

namespace GeoStream.Api.Application.Dtos.Route
{
    public class SearchRouteDto : QueryRequestDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        public byte[]? KmzFile { get; set; }
        public string? GeoJson { get; set; }
        public string? Color { get; set; }
        public TypeOfRoute? TypeOfRoute { get; set; }

        public int? HubId { get; set; }
        public decimal? HubLatitude { get; set; }
        public decimal? HubLongitude { get; set; }
    }
}
