using GeoStream.Dtos.Dependencies;
using System.ComponentModel.DataAnnotations;

namespace GeoStream.Dtos.Application
{
    public class NewAssetDto : BaseDto
    {
        //Search field - AssetRegistry Api
        [Required(ErrorMessage = "Field is required")]
        public string Code { get; set; } = string.Empty;

        //Search field
        [Required(ErrorMessage = "Field is required")]
        public string Emitter { get; set; } = string.Empty;

        public string OwnerDocumentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field is required")]
        public int? RouteId { get; set; }

        public List<SpecialAccessDto> SpecialAccesss { get; set; } = new List<SpecialAccessDto>();
    }
}
