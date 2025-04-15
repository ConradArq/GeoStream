using Microsoft.Extensions.Options;
using MongoDB.Driver;
using GeoStream.Api.Application.Dtos.Emitter;
using GeoStream.Api.Domain.Interfaces.Repositories;

namespace GeoStream.Api.Infrastructure.Persistence.MongoDB.Respositories
{
    public class EmitterRepository : IEmitterRepository
    {
        private readonly IMongoCollection<ResponseEmitterDto> _emitterCollection;
        private readonly IMongoCollection<ResponseIncidentEmitterDto> _incidentCollection;

        public EmitterRepository(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.Connection);
            var db = client.GetDatabase(settings.Value.Database);

            _emitterCollection = db.GetCollection<ResponseEmitterDto>(settings.Value.EmitterCollection);
            _incidentCollection = db.GetCollection<ResponseIncidentEmitterDto>(settings.Value.IncidentCollection);
        }

        public async Task<(IReadOnlyList<ResponseEmitterDto> Data, int TotalItems)> SearchPaginatedAsync(SearchPaginatedEmitterDto request, SortDefinition<ResponseEmitterDto> sortDefinition, FilterDefinition<ResponseEmitterDto> filter)
        {
            var skip = (request.PageNumber - 1) * request.PageSize;

            var data = await _emitterCollection.Find(filter)
                .Sort(sortDefinition)
                .Skip(skip)
                .Limit(request.PageSize)
                .ToListAsync();

            var total = (int)await _emitterCollection.CountDocumentsAsync(filter);

            return (data, total);
        }

        public async Task<(IReadOnlyList<ResponseIncidentEmitterDto> Data, int TotalItems)> SearchIncidentsPaginatedAsync(SearchPaginatedEmitterDto request, SortDefinition<ResponseIncidentEmitterDto> sortDefinition, FilterDefinition<ResponseIncidentEmitterDto> filter)
        {
            var skip = (request.PageNumber - 1) * request.PageSize;

            var data = await _incidentCollection.Find(filter)
                .Sort(sortDefinition)
                .Skip(skip)
                .Limit(request.PageSize)
                .ToListAsync();

            var total = (int)await _incidentCollection.CountDocumentsAsync(filter);

            return (data, total);
        }

        public async Task<IReadOnlyList<ResponseIncidentEmitterDto>> GetAllIncidentsSinceAsync(DateTime fromUtc)
        {
            var filter = Builders<ResponseIncidentEmitterDto>.Filter.Gte(x => x.DateTime, fromUtc);
            return await _incidentCollection.Find(filter).SortByDescending(x => x.DateTime).ToListAsync();
        }

        public async Task<IReadOnlyList<ResponseEmitterDto>> GetAllEmittersSinceAsync(DateTime fromUtc)
        {
            var filter = Builders<ResponseEmitterDto>.Filter.Gte(x => x.DateTime, fromUtc);
            return await _emitterCollection.Find(filter).SortByDescending(x => x.DateTime).ToListAsync();
        }

        public async Task<List<ResponseIncidentEmitterDto>> GetIncidentsForIntervalAsync(DateTime fromUtc, DateTime toUtc)
        {
            var filter = Builders<ResponseIncidentEmitterDto>.Filter.Gte(a => a.DateTime, fromUtc) & Builders<ResponseIncidentEmitterDto>.Filter.Lte(a => a.DateTime, toUtc);
            return await _incidentCollection.Find(filter).ToListAsync();
        }
    }
}