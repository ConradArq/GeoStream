using GeoStream.Api.Application.Interfaces.Strategies;
using GeoStream.Api.Domain.Interfaces.Models;
using GeoStream.Api.Domain.Interfaces.Repositories;

namespace GeoStream.Api.Infrastructure.Persistence.MSSQL.Strategies
{
    /// <summary>
    /// Provides a default implementation for synchronizing a collection of related entities 
    /// within a parent entity, handling additions, updates, and deletions (soft or hard).
    /// 
    /// <para>
    /// Sync logic performs the following steps:
    /// 1. Loads the current and updated collections from the parent entity.
    /// 2. Compares entities by their Id to determine:
    ///    - Which items exist in both (to be preserved or optionally updated),
    ///    - Which items are missing in the updated list (to be soft or hard deleted),
    ///    - Which new items (Id = 0) need to be added.
    /// 3. Applies the specified <see cref="IDeletionStrategy{T}"/> to remove obsolete items.
    /// 4. Adds new items to the current collection.
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">The parent entity type.</typeparam>
    /// <typeparam name="TRelated">The related entity type that implements <see cref="IBaseDomainModel"/>.</typeparam>
    public class DefaultRelatedEntitySyncStrategy<TEntity, TRelated> : IRelatedEntitySyncStrategy<TEntity>
        where TEntity : class
        where TRelated : class, IBaseDomainModel
    {
        private readonly Func<TEntity, ICollection<TRelated>?> _getCollection;
        private readonly Func<TEntity, ICollection<TRelated>?> _getUpdatedCollection;
        private readonly IDeletionStrategy<TRelated>? _deletionStrategy;

        public DefaultRelatedEntitySyncStrategy(
            Func<TEntity, ICollection<TRelated>?> getCollection,
            Func<TEntity, ICollection<TRelated>?> getUpdatedCollection,
            IDeletionStrategy<TRelated>? deletionStrategy = null)
        {
            _getCollection = getCollection;
            _getUpdatedCollection = getUpdatedCollection;
            _deletionStrategy = deletionStrategy;
        }

        public void Sync(TEntity entity, IUnitOfWork unitOfWork)
        {
            var currentItems = _getCollection(entity);
            var updatedItems = _getUpdatedCollection(entity);

            if (currentItems != null && updatedItems != null)
            {
                var updatedIds = updatedItems.Where(x => !IsDefaultId(x.Id)).Select(x => x.Id!).ToHashSet();
                var toDelete = currentItems.Where(existing => !IsDefaultId(existing.Id) && !updatedIds.Contains(existing.Id!)).ToList();
                foreach (var item in toDelete)
                {
                    if (_deletionStrategy != null)
                        _deletionStrategy.Delete(item, unitOfWork);
                    else
                        unitOfWork.Repository<TRelated>().Delete(item);
                }

                foreach (var updated in updatedItems)
                {
                    if (IsDefaultId(updated.Id))
                    {
                        currentItems.Add(updated);
                    }
                    else
                    {
                        var existing = currentItems.FirstOrDefault(e => e.Id == updated.Id);
                        if (existing != null)
                        {
                            // Existing item found — no action needed by default, but updated values could be mapped here if required.                     }
                        }
                    }
                }
            }
        }

        private static bool IsDefaultId(object? id)
        {
            return id == null ||
                   id is int i && i == 0 ||
                   id is long l && l == 0L ||
                   id is Guid g && g == Guid.Empty ||
                   id is string s && string.IsNullOrWhiteSpace(s);
        }
    }
}
