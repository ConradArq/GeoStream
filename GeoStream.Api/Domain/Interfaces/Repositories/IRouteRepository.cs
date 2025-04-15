using GeoStream.Api.Domain.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Route = GeoStream.Api.Domain.Models.Entities.Route;

namespace GeoStream.Api.Domain.Interfaces.Repositories
{
    public interface IRouteRepository : IGenericRepository<Route>
    {
    }
}
