using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeoStream.Api.Application.Dtos.Region
{
    public class SearchRegionDto : RequestDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public int CountryId { get; set; }
    }
}
