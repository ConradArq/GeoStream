
using GeoStream.Api.Domain.Enums;

namespace GeoStream.Api.Application.Dtos.Route
{
    public class ResponseRouteDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public byte[] KmzFile { get; set; } = new byte[0];
        public string GeoJson { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public TypeOfRoute? TypeOfRoute { get; set; }
        public int StatusId { get; set; }
    }
}
