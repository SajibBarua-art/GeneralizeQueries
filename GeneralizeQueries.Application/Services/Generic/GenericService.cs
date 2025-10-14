using GeneralizeQueries.Core.Interfaces;
using GeneralizeQueries.Core.Models;
using Microsoft.Extensions.Logging;

namespace GeneralizeQueries.Application;

/// <summary>
///     Generic service implementation for common business operations with pagination support
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class GenericService<T> : IGenericService<T> where T : class
{
    private readonly ILogger<GenericService<T>> _logger;

    public GenericService(ILogger<GenericService<T>> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<T>> GetAllAsync(IGenericRepository<T> repository)
    {
        _logger.LogInformation("Getting all entities of type {EntityType}", typeof(T).Name);
        return await repository.GetAllAsync();
    }

    public async Task<PagedResult<T>> GetPagedAsync(
        IGenericRepository<T> repository,
        PaginationParameters parameters)
    {
        _logger.LogInformation("Getting paged entities of type {EntityType}, Page: {Page}, PageSize: {PageSize}",
            typeof(T).Name, parameters.Page, parameters.PageSize);
        return await repository.GetPagedAsync(parameters);
    }

    public async Task<long> GetCountAsync(IGenericRepository<T> repository)
    {
        _logger.LogInformation("Getting count of entities of type {EntityType}", typeof(T).Name);
        return await repository.GetCountAsync();
    }

    public async Task<T?> GetByIdAsync(
        IGenericRepository<T> repository,
        Guid id)
    {
        _logger.LogInformation("Getting entity of type {EntityType} by ID: {EntityId}", typeof(T).Name, id);
        return await repository.GetByIdAsync(id);
    }

    public async Task CreateAsync(
        IGenericRepository<T> repository,
        T entity)
    {
        _logger.LogInformation("Creating entity of type {EntityType}", typeof(T).Name);
        await repository.CreateAsync(entity);
    }

    public async Task UpdateAsync(
        IGenericRepository<T> repository,
        T entity)
    {
        _logger.LogInformation("Updating entity of type {EntityType}", typeof(T).Name);
        await repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(
        IGenericRepository<T> repository,
        Guid id)
    {
        _logger.LogInformation("Deleting entity of type {EntityType} with ID: {EntityId}", typeof(T).Name, id);
        await repository.DeleteAsync(id);
    }
}