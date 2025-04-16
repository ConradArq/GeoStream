using Microsoft.AspNetCore.Mvc;
using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.Route;
using GeoStream.Api.Application.Dtos.RouteHub;

namespace GeoStream.Api.Application.Interfaces.Services
{
    public interface IRouteService
    {
        Task<ResponseDto<ResponseRouteDto>> CreateAsync(CreateRouteDto requestDto);
        Task<ResponseDto<ResponseRouteDto>> UpdateAsync(int id, UpdateRouteDto requestDto);
        Task<ResponseDto<List<ResponseRouteDto>>> UpdateRangeAsync([FromBody] List<UpdateRouteDto> requestDtos);
        Task<ResponseDto<object>> DeleteAsync(int id);
        Task<ResponseDto<ResponseRouteDto>> GetAsync(int id);
        Task<ResponseDto<IEnumerable<ResponseRouteDto>>> GetAllAsync(RequestDto? requestDto);
        Task<PaginatedResponseDto<IEnumerable<ResponseRouteDto>>> GetAllPaginatedAsync(PaginationRequestDto requestDto);
        Task<ResponseDto<IEnumerable<ResponseRouteDto>>> SearchAsync(SearchRouteDto requestDto);
        Task<PaginatedResponseDto<IEnumerable<ResponseRouteDto>>> SearchPaginatedAsync(SearchPaginatedRouteDto requestDto);
        Task<ResponseDto<FileStreamResult>> GetKmzFileStreamResult(int routeId);
        Task<ResponseDto<IEnumerable<ResponseRouteHubDto>>> GetAllRouteHubsAsync(RequestDto? requestDto);
    }
}