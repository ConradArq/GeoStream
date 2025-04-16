using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeoStream.Api.Application.Dtos.District
{
    public class SearchDistrictDto : RequestDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public int RegionId { get; set; }
    }
}
