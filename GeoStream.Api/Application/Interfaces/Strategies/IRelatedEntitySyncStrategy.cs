using GeoStream.Api.Domain.Interfaces.Repositories;

namespace GeoStream.Api.Application.Interfaces.Strategies
{
    /// <summary>
    /// Defines a strategy for synchronizing a related entity collection within a parent entity.
    /// </summary>
    /// <typeparam name="TEntity">The parent entity type that contains the related collection to synchronize.</typeparam>
    /// <remarks>
    /// This interface is typically used to update, add, or delete related entities 
    /// during an update operation, based on changes in a corresponding DTO.
    /// </remarks>
    public interface IRelatedEntitySyncStrategy<TEntity>
    {
        void Sync(TEntity entity, IUnitOfWork unitOfWork);
    }
}
