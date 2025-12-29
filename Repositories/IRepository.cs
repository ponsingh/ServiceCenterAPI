using System.Linq.Expressions;

namespace ServiceCenterAPI.Repositories
{
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Get entity by primary key
        /// </summary>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Get all entities
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Find entities by predicate (WHERE clause)
        /// </summary>
        /// <param name="predicate">Filter condition</param>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Get first entity matching condition or null
        /// </summary>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Check if any entity matches condition
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Count entities matching condition
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Add new entity
        /// </summary>
        Task AddAsync(T entity);

        /// <summary>
        /// Add multiple entities
        /// </summary>
        Task AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Update existing entity
        /// </summary>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Delete entity
        /// </summary>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Delete multiple entities
        /// </summary>
        Task DeleteRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Save changes to database
        /// </summary>
        Task SaveChangesAsync();
    }
}
