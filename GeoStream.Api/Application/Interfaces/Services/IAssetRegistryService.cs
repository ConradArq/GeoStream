using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.AssetRegistry;

namespace GeoStream.Api.Application.Interfaces.Services
{
    public interface IAssetRegistryService
    {
        Task<ResponseDto<IEnumerable<ResponseAssetRegistryDto>>> SearchAsync(SearchAssetRegistryDto requestDto);
    }
}