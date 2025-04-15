using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Dtos.Emitter;

namespace GeoStream.Api.Application.Services
{
    public interface IEmitterService
    {
        Task<ResponseDto<IEnumerable<IncidentsIntervalStatisticsDto>>> GetIncidentsIntervalStatisticsAsync(Interval interval);
        Task<ResponseDto<IEnumerable<ResponseIncidentEmitterDto>>> GetAllIncidentsSinceAsync(DateTime fromUtc);
        Task<ResponseDto<IEnumerable<ResponseEmitterDto>>> GetAllEmittersSinceAsync(DateTime fromUtc);
        Task<PaginatedResponseDto<IEnumerable<ResponseIncidentEmitterDto>>> SearchIncidentsPaginatedAsync(SearchPaginatedEmitterDto requestDto);
        Task<PaginatedResponseDto<IEnumerable<ResponseEmitterDto>>> SearchPaginatedAsync(SearchPaginatedEmitterDto requestDto);
    }
}