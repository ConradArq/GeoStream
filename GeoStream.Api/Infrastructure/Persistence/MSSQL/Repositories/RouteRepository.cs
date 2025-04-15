using GeoStream.Api.Domain.Interfaces.Repositories;
using GeoStream.Api.Infrastructure.Persistence.MSSQL;
using Route = GeoStream.Api.Domain.Models.Entities.Route;

namespace GeoStream.Api.Infrastructure.Persistence.MSSQL.Repositories
{
    public class RouteRepository : GenericRepository<Route>, IRouteRepository
    {
        public RouteRepository(GeoStreamDbContext context) : base(context)
        {
        }
    }
}
