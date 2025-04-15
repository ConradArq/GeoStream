using System.ComponentModel.DataAnnotations;

namespace GeoStream.Dtos.Configuration
{
    public class DistrictDto
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
