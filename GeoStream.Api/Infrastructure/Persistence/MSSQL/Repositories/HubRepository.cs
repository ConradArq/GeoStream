using GeoStream.Api.Domain.Models.Entities;
using GeoStream.Api.Domain.Interfaces.Repositories;
using GeoStream.Api.Infrastructure.Persistence.MSSQL;

namespace GeoStream.Api.Infrastructure.Persistence.MSSQL.Repositories
{
    public class HubRepository : GenericRepository<Hub>, IHubRepository
    {
        public HubRepository(GeoStreamDbContext context) : base(context)
        {
        }
    }
}
