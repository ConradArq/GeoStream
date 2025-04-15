using GeoStream.Api.Application.Interfaces.Strategies;
using GeoStream.Api.Domain.Interfaces.Models;
using GeoStream.Api.Domain.Interfaces.Repositories;

namespace GeoStream.Api.Infrastructure.Persistence.MSSQL.Strategies
{
    /// <summary>
    /// A deletion strategy that performs a hard delete by removing the entity from the database context.
    /// </summary>
    /// <typeparam name="T">The type of the entity to delete.</typeparam>
    public class HardDeleteStrategy<T> : IDeletionStrategy<T> where T : class, IBaseDomainModel
    {
        public static readonly HardDeleteStrategy<T> Instance = new();

        private HardDeleteStrategy() { }

        public void Delete(T entity, IUnitOfWork unitOfWork)
        {
            unitOfWork.Repository<T>().Delete(entity);
        }
    }
}
