
using System.ComponentModel;
using GeoStream.Api.Application.Dtos.SpecialAccess;

namespace GeoStream.Api.Application.Dtos.Asset
{
    public class ResponseAssetDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Emitter { get; set; } = string.Empty;
        public string OwnerDocumentNumber { get; set; } = string.Empty;
        public int RouteId { get; set; }
        public int StatusId { get; set; }
        public List<ResponseSpecialAccessDto> SpecialAccesss { get; set; } = new List<ResponseSpecialAccessDto>();
    }
}
