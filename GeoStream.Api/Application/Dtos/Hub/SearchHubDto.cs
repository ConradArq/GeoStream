using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeoStream.Api.Application.Dtos.Hub
{
    public class SearchHubDto : RequestDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? LocationId { get; set; }
    }
}
