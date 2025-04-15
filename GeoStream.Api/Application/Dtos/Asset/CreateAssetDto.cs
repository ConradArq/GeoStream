
using System.ComponentModel;
using GeoStream.Api.Application.Dtos.SpecialAccess;

namespace GeoStream.Api.Application.Dtos.Asset
{
    public class CreateAssetDto
    {
        public string Code { get; set; } = string.Empty;
        public string Emitter { get; set; } = string.Empty;
        public string OwnerDocumentNumber { get; set; } = string.Empty;
        public int RouteId { get; set; }

        [DefaultValue((int)Domain.Enums.Status.Active)]
        public int StatusId { get; set; } = (int)Domain.Enums.Status.Active;
        public List<CreateSpecialAccessDto> SpecialAccesss { get; set; } = new List<CreateSpecialAccessDto>();
    }
}