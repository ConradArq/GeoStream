using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.Scanner;

namespace GeoStream.Api.Application.Interfaces.Services
{
    public interface IScannerService
    {
        Task<ResponseDto<ResponseScannerDto>> CreateAsync(CreateScannerDto requestDto);
        Task<ResponseDto<ResponseScannerDto>> UpdateAsync(int id, UpdateScannerDto requestDto);
        Task<ResponseDto<object>> DeleteAsync(int id);
        Task<ResponseDto<ResponseScannerDto>> GetAsync(int id);
        Task<ResponseDto<IEnumerable<ResponseScannerDto>>> GetAllAsync(RequestDto? requestDto);
        Task<PaginatedResponseDto<IEnumerable<ResponseScannerDto>>> GetAllPaginatedAsync(PaginationRequestDto requestDto);
        Task<ResponseDto<IEnumerable<ResponseScannerDto>>> SearchAsync(SearchScannerDto requestDto);
        Task<PaginatedResponseDto<IEnumerable<ResponseScannerDto>>> SearchPaginatedAsync(SearchPaginatedScannerDto requestDto);
    }
}
