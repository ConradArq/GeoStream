using GeoStream.Api.Domain.Interfaces.Repositories;
using GeoStream.Api.Domain.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoStream.Api.Infrastructure.Persistence.MSSQL;

namespace GeoStream.Api.Infrastructure.Persistence.MSSQL.Repositories
{
    public class AssetRepository : GenericRepository<Asset>, IAssetRepository
    {
        public AssetRepository(GeoStreamDbContext context) : base(context)
        {
        }
    }
}
