using GeneralizeQueries.Core.Models;

namespace GeneralizeQueries.Core.Interfaces;

/// <summary>
///     Generic service interface for common business operations with pagination support
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IGenericService<T> where T : class
{
    /// <summary>
    ///     Gets all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(IGenericRepository<T> repository);

    /// <summary>
    ///     Gets entities with pagination
    /// </summary>
    Task<PagedResult<T>> GetPagedAsync(
        IGenericRepository<T> repository,
        PaginationParameters parameters);

    /// <summary>
    ///     Gets the total count of entities
    /// </summary>
    Task<long> GetCountAsync(IGenericRepository<T> repository);

    /// <summary>
    ///     Gets an entity by its ID
    /// </summary>
    Task<T?> GetByIdAsync(
        IGenericRepository<T> repository,
        Guid id);

    /// <summary>
    ///     Creates a new entity
    /// </summary>
    Task CreateAsync(
        IGenericRepository<T> repository,
        T entity);

    /// <summary>
    ///     Updates an existing entity
    /// </summary>
    Task UpdateAsync(
        IGenericRepository<T> repository,
        T entity);

    /// <summary>
    ///     Deletes an entity by ID
    /// </summary>
    Task DeleteAsync(
        IGenericRepository<T> repository,
        Guid id);
}