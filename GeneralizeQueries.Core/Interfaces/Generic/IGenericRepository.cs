using GeneralizeQueries.Core.Models;

namespace GeneralizeQueries.Core.Interfaces;

/// <summary>
///     Generic repository interface for common CRUD operations with pagination support
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    ///     Gets an entity by its ID
    /// </summary>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    ///     Gets all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    ///     Gets entities with pagination
    /// </summary>
    Task<PagedResult<T>> GetPagedAsync(PaginationParameters parameters);

    /// <summary>
    ///     Gets the total count of entities
    /// </summary>
    Task<long> GetCountAsync();

    /// <summary>
    ///     Creates a new entity
    /// </summary>
    Task CreateAsync(T entity);

    /// <summary>
    ///     Updates an existing entity
    /// </summary>
    Task UpdateAsync(T entity);

    /// <summary>
    ///     Deletes an entity by ID
    /// </summary>
    Task DeleteAsync(Guid id);
}