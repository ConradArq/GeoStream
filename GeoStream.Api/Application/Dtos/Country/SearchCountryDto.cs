using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeoStream.Api.Application.Dtos.Country
{
    public class SearchCountryDto : RequestDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
    }
}
