using GeoStream.Api.Domain.Interfaces.Models;
using GeoStream.Api.Domain.Interfaces.Repositories;

namespace GeoStream.Api.Application.Interfaces.Strategies
{
    /// <summary>
    /// Defines a strategy for deleting an entity, which may be implemented as a hard or soft delete.
    /// </summary>
    public interface IDeletionStrategy<T> where T : class, IBaseDomainModel
    {
        void Delete(T entity, IUnitOfWork unitOfWork);
    }
}
