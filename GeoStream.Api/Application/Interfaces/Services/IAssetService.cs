using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.Asset;

namespace GeoStream.Api.Application.Interfaces.Services
{
    public interface IAssetService
    {
        Task<ResponseDto<ResponseAssetDto>> CreateAsync(CreateAssetDto requestDto);
        Task<ResponseDto<ResponseAssetDto>> UpdateAsync(int id, UpdateAssetDto requestDto);
        Task<ResponseDto<object>> DeleteAsync(int id);
        Task<ResponseDto<ResponseAssetDto>> GetAsync(int id);
        Task<ResponseDto<IEnumerable<ResponseAssetDto>>> GetAllAsync(RequestDto? requestDto);
        Task<PaginatedResponseDto<IEnumerable<ResponseAssetDto>>> GetAllPaginatedAsync(PaginationRequestDto requestDto);
        Task<ResponseDto<IEnumerable<ResponseAssetDto>>> SearchAsync(SearchAssetDto requestDto);
        Task<PaginatedResponseDto<IEnumerable<ResponseAssetDto>>> SearchPaginatedAsync(SearchPaginatedAssetDto requestDto);
    }
}