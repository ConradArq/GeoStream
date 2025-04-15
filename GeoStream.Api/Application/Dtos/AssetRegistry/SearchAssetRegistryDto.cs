
namespace GeoStream.Api.Application.Dtos.AssetRegistry
{
    public class SearchAssetRegistryDto : QueryRequestDto
    {
        public string Code { get; set; } = string.Empty;
    }
}
