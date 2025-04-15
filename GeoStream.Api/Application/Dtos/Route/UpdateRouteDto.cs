using System.ComponentModel;
using GeoStream.Api.Domain.Enums;

namespace GeoStream.Api.Application.Dtos.Route
{
    public class UpdateRouteDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public byte[]? KmzFile { get; set; }
        public string? GeoJson { get; set; }
        public string? Color { get; set; }
        public TypeOfRoute? TypeOfRoute { get; set; }

        [DefaultValue((int)Status.Active)]
        public int? StatusId { get; set; }
    }
}