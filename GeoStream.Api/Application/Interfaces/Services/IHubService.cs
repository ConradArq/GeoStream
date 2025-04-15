using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.Hub;

namespace GeoStream.Api.Application.Interfaces.Services
{
    public interface IHubService
    {
        Task<ResponseDto<ResponseHubDto>> CreateAsync(CreateHubDto requestDto);
        Task<ResponseDto<ResponseHubDto>> UpdateAsync(int id, UpdateHubDto requestDto);
        Task<ResponseDto<object>> DeleteAsync(int id);
        Task<ResponseDto<ResponseHubDto>> GetAsync(int id);
        Task<ResponseDto<IEnumerable<ResponseHubDto>>> GetAllAsync(QueryRequestDto? requestDto);
        Task<PaginatedResponseDto<IEnumerable<ResponseHubDto>>> GetAllPaginatedAsync(PaginationRequestDto requestDto);
        Task<ResponseDto<IEnumerable<ResponseHubDto>>> SearchAsync(SearchHubDto requestDto);
        Task<PaginatedResponseDto<IEnumerable<ResponseHubDto>>> SearchPaginatedAsync(SearchPaginatedHubDto requestDto);
    }
}