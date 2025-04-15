using GeoStream.Attributes;
using System.ComponentModel.DataAnnotations;
using GeoStream.Dtos.Enums;

namespace GeoStream.Dtos.Configuration
{
    public class RouteDto : BaseDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Field is required")]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [RequiredByteArray(ErrorMessage = "Field is required")]
        public byte[] KmzFile { get; set; } = new byte[0];
        public string GeoJson { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public TypeOfRoute? TypeOfRoute { get; set; }
    }

    public class RouteDtoMudSelect
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
