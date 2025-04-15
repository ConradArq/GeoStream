using GeoStream.Dtos.Configuration;

namespace GeoStream.Dtos.Application
{
    public class AssetDto : BaseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Emitter { get; set; } = string.Empty;
        public string OwnerDocumentNumber { get; set; } = string.Empty;    
    }
}
