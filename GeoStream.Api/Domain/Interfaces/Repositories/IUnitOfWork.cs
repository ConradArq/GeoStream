using GeoStream.Api.Domain.Interfaces.Models;

namespace GeoStream.Api.Domain.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        IRouteRepository RouteRepository { get; }
        IHubRepository HubRepository { get; }
        IAssetRepository AssetRepository { get; }
        IScannerRepository ScannerRepository { get; }
        ICountryRepository CountryRepository { get; }
        IRegionRepository RegionRepository { get; }
        IDistrictRepository DistrictRepository { get; }
        IAssetEmitterRepository AssetEmitterRepository { get; }
        ILocationRepository LocationRepository { get; }
        IEmitterRepository EmitterRepository { get; }
        IRouteHubRepository RouteHubRepository { get; }
        Task CompleteTransactionAsync(Func<Task> functionTransaction);
        void Dispose();
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class, IBaseDomainModel;
        object GetRepository(Type entityType);
        Task<int> SaveAsync();
    }
}