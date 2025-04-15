using GeoStream.Api.Domain.Models.Entities;
using GeoStream.Api.Domain.Interfaces.Repositories;
using GeoStream.Api.Infrastructure.Persistence.MSSQL;

namespace GeoStream.Api.Infrastructure.Persistence.MSSQL.Repositories
{
    public class CountryRepository : GenericRepository<Country>, ICountryRepository
    {
        public CountryRepository(GeoStreamDbContext context) : base(context)
        {
        }
    }
}
