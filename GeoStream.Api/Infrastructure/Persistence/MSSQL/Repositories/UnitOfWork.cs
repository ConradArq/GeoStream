using GeoStream.Api.Domain.Interfaces.Models;
using GeoStream.Api.Domain.Interfaces.Repositories;
using GeoStream.Api.Infrastructure.Persistence.MSSQL;

namespace GeoStream.Api.Infrastructure.Persistence.MSSQL.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private Dictionary<string, object> _repositories = new();
        private readonly GeoStreamDbContext _context;

        private IHubRepository _hubRepository;
        private IAssetRepository _assetRepository;
        private IRouteRepository _routeRepository;
        private IScannerRepository _scannerRepository;
        private ICountryRepository _countryRepository;
        private IRegionRepository _regionRepository;
        private IDistrictRepository _districtRepository;
        private IAssetEmitterRepository _assetEmitterRepository;
        private ILocationRepository _locationRepository;
        private IEmitterRepository _emitterRepository;
        private IRouteHubRepository _routeHubRepository;

        public UnitOfWork(GeoStreamDbContext context, IHubRepository hubRepository, IAssetRepository assetRepository, IRouteRepository routeRepository, IScannerRepository scannerRepository, ICountryRepository countryRepository, IRegionRepository regionRepository, IDistrictRepository districtRepository, IAssetEmitterRepository assetEmitterRepository, ILocationRepository locationRepository, IEmitterRepository emitterRepository, IRouteHubRepository routeHubRepository)
        {
            _context = context;
            _hubRepository = hubRepository;
            _assetRepository = assetRepository;
            _routeRepository = routeRepository;
            _scannerRepository = scannerRepository;
            _countryRepository = countryRepository;
            _regionRepository = regionRepository;
            _districtRepository = districtRepository;
            _assetEmitterRepository = assetEmitterRepository;
            _locationRepository = locationRepository;
            _emitterRepository = emitterRepository;
            _routeHubRepository = routeHubRepository;
        }

        public IHubRepository HubRepository => _hubRepository;
        public IAssetRepository AssetRepository => _assetRepository;
        public IRouteRepository RouteRepository => _routeRepository;
        public IScannerRepository ScannerRepository => _scannerRepository;
        public ICountryRepository CountryRepository => _countryRepository;
        public IRegionRepository RegionRepository => _regionRepository;
        public IDistrictRepository DistrictRepository => _districtRepository;
        public IAssetEmitterRepository AssetEmitterRepository => _assetEmitterRepository;
        public ILocationRepository LocationRepository => _locationRepository;
        public IEmitterRepository EmitterRepository => _emitterRepository;
        public IRouteHubRepository RouteHubRepository => _routeHubRepository;

        /// <summary>
        /// Centralizes save logic, ensuring that all changes to the context are persisted at once.
        /// </summary>
        public async Task<int> SaveAsync() => await _context.SaveChangesAsync();

        /// <summary>
        /// Executes a transaction, useful when you need to perform multiple operations that require intermediate saves or interdependent 
        /// actions. For example, when saving data to one entity, using its value for further operations, and then saving another entity.
        /// This also ensures atomicity when performing actions such as saving to the database and 
        /// sending an email, maintaining consistency across multiple database actions and external processes.
        /// </summary>
        public async Task CompleteTransactionAsync(Func<Task> functionTransaction)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await functionTransaction();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"An error occurred while saving data for the context: {_context.GetType().FullName}", ex);
            }
        }

        /// <summary>
        /// Provides a generic repository instance for the specified TEntity type which simplifies access to basic CRUD operations 
        /// for any entity type, allowing developers to manage multiple entities without writing repetitive code for each repository.
        /// </summary>
        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class, IBaseDomainModel
        {
            var type = typeof(TEntity).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new GenericRepository<TEntity>(_context);
                _repositories[type] = repositoryInstance;
            }

            return (IGenericRepository<TEntity>)_repositories[type];
        }

        public object GetRepository(Type entityType)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            if (!typeof(IBaseDomainModel).IsAssignableFrom(entityType))
                throw new InvalidOperationException($"The type {entityType.Name} is not a valid domain model.");

            var typeName = entityType.Name;

            if (!_repositories.ContainsKey(typeName))
            {
                var repositoryType = typeof(GenericRepository<>).MakeGenericType(entityType);
                var repositoryInstance = Activator.CreateInstance(repositoryType, _context);

                if (repositoryInstance == null)
                    throw new InvalidOperationException($"Could not create repository instance for type {entityType.Name}.");

                _repositories[typeName] = repositoryInstance;
            }

            return _repositories[typeName];
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
