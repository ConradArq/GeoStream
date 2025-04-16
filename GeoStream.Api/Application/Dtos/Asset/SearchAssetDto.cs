
using System.ComponentModel;

namespace GeoStream.Api.Application.Dtos.Asset
{
    public class SearchAssetDto : RequestDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Emitter { get; set; }
        public string? OwnerDocumentNumber { get; set; }
        public int? RouteId { get; set; }
    }
}
