using System.ComponentModel;
using GeoStream.Api.Domain.Enums;

namespace GeoStream.Api.Application.Dtos.Route
{
    public class CreateRouteDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public byte[] KmzFile { get; set; } = new byte[0];
        public string GeoJson { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public TypeOfRoute? TypeOfRoute { get; set; }

        [DefaultValue((int)Domain.Enums.Status.Active)]
        public int StatusId { get; set; }
    }
}