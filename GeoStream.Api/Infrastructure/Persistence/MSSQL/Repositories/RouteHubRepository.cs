using Microsoft.EntityFrameworkCore;
using GeoStream.Api.Domain.Interfaces.Repositories;
using GeoStream.Api.Domain.Models.Entities;

namespace GeoStream.Api.Infrastructure.Persistence.MSSQL.Repositories
{
    public class RouteHubRepository : GenericRepository<RouteHub>, IRouteHubRepository
    {
        public RouteHubRepository(GeoStreamDbContext context) : base(context)
        {
        }
    }
}
