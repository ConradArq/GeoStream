using MongoDB.Driver;
using GeoStream.Api.Application.Dtos.Emitter;

namespace GeoStream.Api.Domain.Interfaces.Repositories
{
    public interface IEmitterRepository
    {
        Task<List<ResponseIncidentEmitterDto>> GetIncidentsForIntervalAsync(DateTime fromUtc, DateTime toUtc);
        Task<IReadOnlyList<ResponseIncidentEmitterDto>> GetAllIncidentsSinceAsync(DateTime fromUtc);
        Task<IReadOnlyList<ResponseEmitterDto>> GetAllEmittersSinceAsync(DateTime fromUtc);
        Task<(IReadOnlyList<ResponseIncidentEmitterDto> Data, int TotalItems)> SearchIncidentsPaginatedAsync(SearchPaginatedEmitterDto request, SortDefinition<ResponseIncidentEmitterDto> sortDefinition, FilterDefinition<ResponseIncidentEmitterDto> filter);
        Task<(IReadOnlyList<ResponseEmitterDto> Data, int TotalItems)> SearchPaginatedAsync(SearchPaginatedEmitterDto request, SortDefinition<ResponseEmitterDto> sortDefinition, FilterDefinition<ResponseEmitterDto> filter);
    }
}