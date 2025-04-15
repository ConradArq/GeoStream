using System.Linq.Expressions;

namespace GeoStream.Api.Domain.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Retrieves a list of entities from the database with optional filtering, eager loading, and ordering.
        /// Use this method when you need to retrieve the full entity model (`T`).
        /// </summary>
        /// <param name="predicate">
        ///     (Optional) A filter expression to apply to the query.
        ///     Example: <c>x => x.IsActive</c>
        /// </param>
        /// <param name="disableTracking">
        ///     (Optional, default: <c>true</c>) Whether to disable EF Core change tracking for better read performance.
        /// </param>
        /// <param name="orderBy">
        ///     (Optional) A function to apply ordering.
        ///     Example: <c>q => q.OrderBy(x => x.Name)</c>
        /// </param>
        /// <param name="includes">
        ///     (Optional) One or more functions to include related entities using <c>.Include()</c> and <c>.ThenInclude()</c>.
        ///     Each function can chain nested navigation properties as needed.
        ///     <br/><br/>
        ///     <b>Example usage (with positional arguments):</b>
        ///     <code>
        ///     q => q.Include(x => x.RelatedEntity).ThenInclude(y => y.SubRelatedEntity),
        ///     q => q.Include(x => x.AnotherRelation)
        ///     </code>
        ///     <br/><br/>
        ///     <b>Note:</b> If you're using named parameters and want to pass includes this way,
        ///     use the helper method <c>Includes<T>()</c> to avoid generic type inference issues:
        ///     <code>
        ///     includes: Includes<T>(
        ///         q => q.Include(x => x.RelatedEntity).ThenInclude(y => y.SubRelatedEntity),
        ///         q => q.Include(x => x.AnotherRelation)
        ///     )
        ///     </code>
        ///     This ensures compatibility with named arguments and keeps the code clean.
        /// </param>
        /// <returns>A read-only list of full entity models of type <c>T</c>.</returns>
        Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>>? predicate = null,
            bool disableTracking = true,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Func<IQueryable<T>, IQueryable<T>>[] includes);

        /// <summary>
        /// Retrieves a list of projected results from the database (e.g., DTOs) with optional filtering, eager loading, and ordering.
        /// Use this method when you want to project to a lightweight type (`TResult`) inside the query (e.g., for performance or custom shaping).
        /// </summary>
        /// <typeparam name="TResult">The type to project each entity into (e.g., a DTO).</typeparam>
        /// <param name="predicate">
        ///     (Optional) A filter expression to apply.
        ///     Example: <c>x => x.IsActive</c>
        /// </param>
        /// <param name="disableTracking">
        ///     (Optional, default: <c>true</c>) Disables EF Core tracking for better performance.
        /// </param>
        /// <param name="orderBy">
        ///     (Optional) A function to order the query.
        ///     Example: <c>q => q.OrderBy(x => x.Name)</c>
        /// </param>
        /// <param name="selector">
        ///     A projection expression to map from the entity to the desired result.
        ///     Example: <c>q => q.Select(x => new MyDto { ... })</c>
        /// </param>
        /// <param name="includes">
        ///     (Optional) One or more functions to include related entities using <c>.Include()</c> and <c>.ThenInclude()</c>.
        ///     Each function can chain nested navigation properties as needed.
        ///     <br/><br/>
        ///     <b>Example usage (with positional arguments):</b>
        ///     <code>
        ///     q => q.Include(x => x.RelatedEntity).ThenInclude(y => y.SubRelatedEntity),
        ///     q => q.Include(x => x.AnotherRelation)
        ///     </code>
        ///     <br/><br/>
        ///     <b>Note:</b> If you're using named parameters and want to pass includes this way,
        ///     use the helper method <c>Includes<T>()</c> to avoid generic type inference issues:
        ///     <code>
        ///     includes: Includes<T>(
        ///         q => q.Include(x => x.RelatedEntity).ThenInclude(y => y.SubRelatedEntity),
        ///         q => q.Include(x => x.AnotherRelation)
        ///     )
        ///     </code>
        ///     This ensures compatibility with named arguments and keeps the code clean.
        /// </param>
        /// <returns>A list of projected results of type <c>TResult</c>.</returns>
        Task<IReadOnlyList<TResult>> GetAsync<TResult>(
            Expression<Func<T, bool>>? predicate = null,
            bool disableTracking = true,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IQueryable<TResult>>? selector = null,
            params Func<IQueryable<T>, IQueryable<T>>[] includes);

        /// <summary>
        /// Retrieves a paginated list of full entities from the database with optional filtering, eager loading, and ordering.
        /// Use this when you need both the data and total count for pagination.
        /// </summary>
        /// <param name="pageNumber">The current page number (1-based). Defaults to 1 if ≤ 0.</param>
        /// <param name="pageSize">The number of records per page. Must be greater than 0.</param>
        /// <param name="predicate">
        ///     (Optional) A filter to apply.
        ///     Example: <c>x => x.IsActive</c>
        /// </param>
        /// <param name="disableTracking">
        ///     (Optional, default: <c>true</c>) Disables change tracking for better performance.
        /// </param>
        /// <param name="orderBy">
        ///     (Optional) A function to order the query.
        ///     Example: <c>q => q.OrderBy(x => x.CreatedAt)</c>
        /// </param>
        /// <param name="includes">
        ///     (Optional) One or more functions to include related entities using <c>.Include()</c> and <c>.ThenInclude()</c>.
        ///     Each function can chain nested navigation properties as needed.
        ///     <br/><br/>
        ///     <b>Example usage (with positional arguments):</b>
        ///     <code>
        ///     q => q.Include(x => x.RelatedEntity).ThenInclude(y => y.SubRelatedEntity),
        ///     q => q.Include(x => x.AnotherRelation)
        ///     </code>
        ///     <br/><br/>
        ///     <b>Note:</b> If you're using named parameters and want to pass includes this way,
        ///     use the helper method <c>Includes<T>()</c> to avoid generic type inference issues:
        ///     <code>
        ///     includes: Includes<T>(
        ///         q => q.Include(x => x.RelatedEntity).ThenInclude(y => y.SubRelatedEntity),
        ///         q => q.Include(x => x.AnotherRelation)
        ///     )
        ///     </code>
        ///     This ensures compatibility with named arguments and keeps the code clean.
        /// </param>
        /// <returns>
        ///     A tuple:
        ///     - <c>Data</c>: The paginated list of entities.
        ///     - <c>TotalItems</c>: The total number of matching records.
        /// </returns>
        Task<(IReadOnlyList<T> Data, int TotalItems)> GetPaginatedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            bool disableTracking = true,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Func<IQueryable<T>, IQueryable<T>>[] includes);

        /// <summary>
        /// Retrieves a paginated list of projected results (`TResult`) from the database.
        /// Use this to return lightweight DTOs in paginated form for APIs or UIs.
        /// </summary>
        /// <typeparam name="TResult">The type to project into (e.g., a DTO).</typeparam>
        /// <param name="pageNumber">The current page number (1-based). Defaults to 1 if ≤ 0.</param>
        /// <param name="pageSize">The number of records per page. Must be greater than 0.</param>
        /// <param name="predicate">
        ///     (Optional) A filter to apply.
        /// </param>
        /// <param name="disableTracking">
        ///     (Optional, default: <c>true</c>) Disables change tracking.
        /// </param>
        /// <param name="orderBy">
        ///     (Optional) A function to order the query.
        /// </param>
        /// <param name="selector">
        ///     A projection expression to return <c>TResult</c> instead of the full entity.
        ///     Example: <c>q => q.Select(x => new MyDto { x.Id, x.Name })</c>
        /// </param>
        /// <param name="includes">
        ///     (Optional) One or more functions to include related entities using <c>.Include()</c> and <c>.ThenInclude()</c>.
        ///     Each function can chain nested navigation properties as needed.
        ///     <br/><br/>
        ///     <b>Example usage (with positional arguments):</b>
        ///     <code>
        ///     q => q.Include(x => x.RelatedEntity).ThenInclude(y => y.SubRelatedEntity),
        ///     q => q.Include(x => x.AnotherRelation)
        ///     </code>
        ///     <br/><br/>
        ///     <b>Note:</b> If you're using named parameters and want to pass includes this way,
        ///     use the helper method <c>Includes<T>()</c> to avoid generic type inference issues:
        ///     <code>
        ///     includes: Includes<T>(
        ///         q => q.Include(x => x.RelatedEntity).ThenInclude(y => y.SubRelatedEntity),
        ///         q => q.Include(x => x.AnotherRelation)
        ///     )
        ///     </code>
        ///     This ensures compatibility with named arguments and keeps the code clean.
        /// </param>
        /// <returns>
        ///     A tuple:
        ///     - <c>Data</c>: The paginated list of projected results.
        ///     - <c>TotalItems</c>: The total number of records.
        /// </returns>
        Task<(IReadOnlyList<TResult> Data, int TotalItems)> GetPaginatedAsync<TResult>(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            bool disableTracking = true,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IQueryable<TResult>>? selector = null,
            params Func<IQueryable<T>, IQueryable<T>>[] includes);

        /// <summary>
        /// Retrieves a single entity by its ID, with optional eager loading.
        /// Use this when you need the full entity and know its ID.
        /// </summary>
        /// <param name="id">The entity’s unique identifier.</param>
        /// <param name="disableTracking">
        ///     (Optional, default: <c>false</c>) Disables EF tracking.
        /// </param>
        /// <param name="includes">
        ///     (Optional) One or more functions to include related entities using <c>.Include()</c> and <c>.ThenInclude()</c>.
        ///     Each function can chain nested navigation properties as needed.
        ///     <br/><br/>
        ///     <b>Example usage (with positional arguments):</b>
        ///     <code>
        ///     q => q.Include(x => x.RelatedEntity).ThenInclude(y => y.SubRelatedEntity),
        ///     q => q.Include(x => x.AnotherRelation)
        ///     </code>
        ///     <br/><br/>
        ///     <b>Note:</b> If you're using named parameters and want to pass includes this way,
        ///     use the helper method <c>Includes<T>()</c> to avoid generic type inference issues:
        ///     <code>
        ///     includes: Includes<T>(
        ///         q => q.Include(x => x.RelatedEntity).ThenInclude(y => y.SubRelatedEntity),
        ///         q => q.Include(x => x.AnotherRelation)
        ///     )
        ///     </code>
        ///     This ensures compatibility with named arguments and keeps the code clean.
        /// </param>
        /// <returns>The entity if found; otherwise, <c>null</c>.</returns>
        Task<T?> GetSingleAsync(
            object id,
            bool disableTracking = false,
            params Func<IQueryable<T>, IQueryable<T>>[] includes);

        /// <summary>
        /// Retrieves a single projected result (`TResult`) by entity ID, with optional eager loading.
        /// Use this for optimized lookups when you only need specific fields, not the full entity.
        /// </summary>
        /// <typeparam name="TResult">The type to project into (e.g., DTO).</typeparam>
        /// <param name="id">The entity’s unique identifier.</param>
        /// <param name="selector">
        ///     A projection expression to return <c>TResult</c>.
        ///     Example: <c>q => q.Select(x => new MyDto { x.Id, x.Name })</c>
        /// </param>
        /// <param name="disableTracking">
        ///     (Optional, default: <c>false</c>) Disables EF tracking.
        /// </param>
        /// <param name="includes">
        ///     (Optional) One or more functions to include related entities using <c>.Include()</c> and <c>.ThenInclude()</c>.
        ///     Each function can chain nested navigation properties as needed.
        ///     <br/><br/>
        ///     <b>Example usage (with positional arguments):</b>
        ///     <code>
        ///     q => q.Include(x => x.RelatedEntity).ThenInclude(y => y.SubRelatedEntity),
        ///     q => q.Include(x => x.AnotherRelation)
        ///     </code>
        ///     <br/><br/>
        ///     <b>Note:</b> If you're using named parameters and want to pass includes this way,
        ///     use the helper method <c>Includes<T>()</c> to avoid generic type inference issues:
        ///     <code>
        ///     includes: Includes<T>(
        ///         q => q.Include(x => x.RelatedEntity).ThenInclude(y => y.SubRelatedEntity),
        ///         q => q.Include(x => x.AnotherRelation)
        ///     )
        ///     </code>
        ///     This ensures compatibility with named arguments and keeps the code clean.
        /// </param>
        /// <returns>The projected result if found; otherwise, <c>null</c>.</returns>
        Task<TResult?> GetSingleAsync<TResult>(
            object id,
            Func<IQueryable<T>, IQueryable<TResult>> selector,
            bool disableTracking = false,
            params Func<IQueryable<T>, IQueryable<T>>[] includes);

        /// <summary>
        /// Creates a new entity in the database.
        /// </summary>
        /// <param name="entity">The entity to be created.</param>
        /// <returns>The created entity.</returns>
        T Create(T entity);

        /// <summary>
        /// Creates multiple entities in the database in a single transaction.
        /// </summary>
        /// <param name="entities">The collection of entities to create.</param>
        /// <returns>The created entities.</returns>
        ICollection<T> CreateRange(ICollection<T> entities);

        /// <summary>
        /// Deletes an entity from the database.
        /// </summary>
        /// <param name="entity">The entity to be deleted.</param>
        void Delete(T entity);

        /// <summary>
        /// Deletes multiple entities from the database in a single transaction.
        /// </summary>
        /// <param name="entities">The collection of entities to delete.</param>
        void DeleteRange(ICollection<T> entities);

        /// <summary>
        /// Updates an entity in the database.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>The updated entity.</returns>
        T Update(T entity);

        /// <summary>
        /// Updates multiple entities in a single transaction.
        /// </summary>
        /// <param name="entities">The collection of entities to update.</param>
        /// <returns>The updated entities.</returns>
        ICollection<T> UpdateRange(ICollection<T> entities);

        /// <summary>
        /// Reloads the specified entity from the database, refreshing its values.
        /// Optionally, a specific property can be reloaded by providing the property expression.
        /// </summary>
        /// <param name="entity">The entity to reload from the database.</param>
        /// <param name="property">Optional expression to specify a particular property to reload. If null, the entire entity is reloaded.</param>
        /// <typeparam name="TProperty">The type of the property to reload, if specified.</typeparam>
        void Reload<TProperty>(T entity, Expression<Func<T, TProperty?>>? property = null) where TProperty : class;
    }
}