using GeneralizeQueries.Core.Interfaces;
using GeneralizeQueries.Core.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Platform.Infrastructure.Core.Domain;

namespace GeneralizeQueries.Infrastructure.Data;

/// <summary>
///     Generic MongoDB repository implementation for common CRUD operations with pagination support
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class GenericMongoRepository<T> : IGenericRepository<T> where T : AggregateRoot
{
    protected readonly IMongoCollection<T> _collection;
    protected readonly ILogger _logger;

    public GenericMongoRepository(
        IMongoDatabase database,
        string collectionName,
        ILogger logger)
    {
        _collection = database.GetCollection<T>(collectionName);
        _logger = logger;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting entity of type {EntityType} by ID: {Id}", typeof(T).Name, id);
        var filter = Builders<T>.Filter.Eq("_id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        _logger.LogInformation("Getting all entities of type {EntityType}", typeof(T).Name);
        return await _collection.Find(_ => true).ToListAsync();
    }

    public virtual async Task<PagedResult<T>> GetPagedAsync(PaginationParameters parameters)
    {
        _logger.LogInformation("Getting paged entities of type {EntityType}, Page: {Page}, PageSize: {PageSize}",
            typeof(T).Name, parameters.Page, parameters.PageSize);
        parameters.ValidateAndSetDefaults();

        var filter = Builders<T>.Filter.Empty;
        var totalCount = await _collection.CountDocumentsAsync(filter);

        var query = _collection.Find(filter);

        var resultCursor = await _collection.FindAsync(filter);
        var testItems = resultCursor.ToList();

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            var sortField = GetSortField(parameters.SortBy);
            var sortDefinition = parameters.SortDirection.ToLower() == "desc"
                ? Builders<T>.Sort.Descending(sortField)
                : Builders<T>.Sort.Ascending(sortField);
            query = query.Sort(sortDefinition);
        }

        // Apply pagination
        var skip = (parameters.Page - 1) * parameters.PageSize;
        var items = await query.Skip(skip).Limit(parameters.PageSize).ToListAsync();

        return PagedResult<T>.Create(items, parameters.Page, parameters.PageSize, totalCount);
    }

    public virtual async Task<long> GetCountAsync()
    {
        _logger.LogInformation("Getting count of entities of type {EntityType}", typeof(T).Name);
        return await _collection.CountDocumentsAsync(_ => true);
    }

    public virtual async Task CreateAsync(T entity)
    {
        _logger.LogInformation("Creating entity of type {EntityType}", typeof(T).Name);
        await _collection.InsertOneAsync(entity);
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _logger.LogInformation("Updating entity of type {EntityType} with ID: {Id}", typeof(T).Name, entity.Id);
        var filter = Builders<T>.Filter.Eq(e => e.Id, entity.Id);

        await _collection.ReplaceOneAsync(filter, entity);
    }


    public virtual async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting entity of type {EntityType} with ID: {Id}", typeof(T).Name, id);
        var filter = Builders<T>.Filter.Eq("_id", id);
        var result = await _collection.DeleteOneAsync(filter);
        if (!result.IsAcknowledged)
        {
            _logger.LogError("Failed to delete entity of type {EntityType} with ID: {Id}", typeof(T).Name, id);
            throw new InvalidOperationException("Failed to delete entity from database");
        }
    }

    /// <summary>
    ///     Maps sort field names to MongoDB field paths. Override this method in derived classes
    ///     to provide custom field mapping for nested properties.
    /// </summary>
    protected virtual string GetSortField(string sortBy)
    {
        // Default implementation - can be overridden in derived classes
        // for custom field mapping (e.g., nested properties)
        return sortBy;
    }
}