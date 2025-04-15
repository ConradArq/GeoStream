using GeoStream.Api.Domain.Interfaces.Repositories;
using GeoStream.Api.Domain.Models.Entities;

namespace GeoStream.Api.Infrastructure.Persistence.MSSQL.Repositories
{
    public class ScannerRepository : GenericRepository<Scanner>, IScannerRepository
    {
        public ScannerRepository(GeoStreamDbContext context) : base(context)
        {
        }
    }
}
