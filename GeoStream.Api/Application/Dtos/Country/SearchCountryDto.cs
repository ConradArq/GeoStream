using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeoStream.Api.Application.Dtos.Country
{
    public class SearchCountryDto : QueryRequestDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
    }
}
