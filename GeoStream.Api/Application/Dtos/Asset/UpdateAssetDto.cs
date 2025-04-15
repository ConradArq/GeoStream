
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GeoStream.Api.Application.Dtos.SpecialAccess;

namespace GeoStream.Api.Application.Dtos.Asset
{
    public class UpdateAssetDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Emitter { get; set; }
        public string? OwnerDocumentNumber { get; set; }
        public int? RouteId { get; set; }

        [DefaultValue((int)Domain.Enums.Status.Active)]
        public int StatusId { get; set; } = (int)Domain.Enums.Status.Active;
        public List<UpdateSpecialAccessDto>? SpecialAccesss { get; set; }
    }
}