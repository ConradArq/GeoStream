using Microsoft.EntityFrameworkCore;
using GeoStream.Api.Domain.Interfaces.Models;
using GeoStream.Api.Domain.Interfaces.Repositories;
using System.Linq.Expressions;
using GeoStream.Api.Infrastructure.Persistence.MSSQL;

namespace GeoStream.Api.Infrastructure.Persistence.MSSQL.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class, IBaseDomainModel
    {
        protected readonly GeoStreamDbContext _context;

        public GenericRepository(GeoStreamDbContext context)
        {
            _context = context;
        }

        public virtual async Task<IReadOnlyList<TResult>> GetAsync<TResult>(
            Expression<Func<T, bool>>? predicate = null,
            bool disableTracking = true,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IQueryable<TResult>>? selector = null,
            params Func<IQueryable<T>, IQueryable<T>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();

            if (disableTracking)
                query = query.AsNoTracking();

            foreach (var include in includes)
            {
                query = include(query);
            }

            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                query = orderBy(query);

            if (selector != null)
                return await selector(query).ToListAsync();

            throw new InvalidOperationException("Selector must be provided when using projection.");
        }

        public virtual async Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>>? predicate = null,
            bool disableTracking = true,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Func<IQueryable<T>, IQueryable<T>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();

            if (disableTracking)
                query = query.AsNoTracking();

            foreach (var include in includes)
            {
                query = include(query);
            }

            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                query = orderBy(query);

            return await query.ToListAsync();
        }

        public virtual async Task<(IReadOnlyList<TResult> Data, int TotalItems)> GetPaginatedAsync<TResult>(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            bool disableTracking = true,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IQueryable<TResult>>? selector = null,
            params Func<IQueryable<T>, IQueryable<T>>[] includes)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));
            if (selector == null) throw new ArgumentNullException(nameof(selector), "Selector is required for projection.");

            IQueryable<T> query = _context.Set<T>();

            if (disableTracking)
                query = query.AsNoTracking();

            foreach (var include in includes)
                query = include(query);

            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                query = orderBy(query);

            var totalItems = await query.CountAsync();
            var pagedQuery = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var projected = selector(pagedQuery);
            var pagedResult = await projected.ToListAsync();

            return (pagedResult, totalItems);
        }

        public virtual async Task<(IReadOnlyList<T> Data, int TotalItems)> GetPaginatedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            bool disableTracking = true,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Func<IQueryable<T>, IQueryable<T>>[] includes)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

            IQueryable<T> query = _context.Set<T>();

            if (disableTracking)
                query = query.AsNoTracking();

            foreach (var include in includes)
                query = include(query);

            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                query = orderBy(query);

            var totalItems = await query.CountAsync();

            var paginatedItems = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (paginatedItems, totalItems);
        }

        public virtual async Task<TResult?> GetSingleAsync<TResult>(
            object id,
            Func<IQueryable<T>, IQueryable<TResult>> selector,
            bool disableTracking = false,
            params Func<IQueryable<T>, IQueryable<T>>[] includes)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            IQueryable<T> query = _context.Set<T>();

            if (disableTracking)
                query = query.AsNoTracking();

            foreach (var include in includes)
                query = include(query);

            var filtered = query.Where(e => e.Id.Equals(id));

            return await selector(filtered).FirstOrDefaultAsync();
        }

        public virtual async Task<T?> GetSingleAsync(
            object id,
            bool disableTracking = false,
            params Func<IQueryable<T>, IQueryable<T>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();

            if (disableTracking)
                query = query.AsNoTracking();

            foreach (var include in includes)
                query = include(query);

            return await query.FirstOrDefaultAsync(e => e.Id.Equals(id));
        }

        public virtual T Create(T entity)
        {
            _context.Set<T>().Add(entity);
            return entity;
        }

        public virtual ICollection<T> CreateRange(ICollection<T> entities)
        {
            foreach (var entity in entities)
            {
                _context.Set<T>().Add(entity);
            }
            return entities;
        }

        public virtual T Update(T entity)
        {
            _context.Set<T>().Update(entity);
            return entity;
        }

        public virtual ICollection<T> UpdateRange(ICollection<T> entities)
        {
            foreach (var entity in entities)
            {
                Update(entity);
            }

            return entities;
        }

        public virtual void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public virtual void DeleteRange(ICollection<T> entities)
        {
            foreach (var entity in entities)
            {
                Delete(entity);
            }
        }

        public virtual void Reload<TProperty>(T entity, Expression<Func<T, TProperty?>>? property = null) where TProperty : class
        {
            var entry = _context.Entry(entity);

            if (property == null)
            {
                // Reload the entire entity itself
                entry.Reload();
            }
            else if (typeof(TProperty).IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(typeof(TProperty).GetGenericTypeDefinition()))
            {
                // Reload a collection navigation property
                var collectionProperty = property as Expression<Func<T, IEnumerable<TProperty>>>;

                if (collectionProperty != null)
                {
                    entry.Collection(collectionProperty).Load();
                }
            }
            else
            {
                // Reload a reference navigation property
                entry.Reference(property).Load();
            }
        }
    }
}
