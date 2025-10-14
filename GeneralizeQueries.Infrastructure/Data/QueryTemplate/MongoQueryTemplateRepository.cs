using GeneralizeQueries.Core.Entities;
using GeneralizeQueries.Core.Interfaces;
using GeneralizeQueries.Core.Models;
using GeneralizeQueries.Infrastructure.Serialization;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace GeneralizeQueries.Infrastructure.Data;

public class MongoQueryTemplateRepository : IQueryTemplateRepository
{
    private readonly ILogger _logger;
    private readonly IMongoCollection<QueryTemplate> _templatesCollection;

    // The static constructor with BsonClassMap registrations for handling
    // inconsistent data types (e.g., boolean vs. string) remains unchanged.
    static MongoQueryTemplateRepository()
    {
        BsonClassMap.RegisterClassMap<TemplateDetails>(cm =>
        {
            cm.AutoMap();
            cm.GetMemberMap(c => c.IsDynamicFilter).SetSerializer(new BooleanOrStringSerializer());
        });

        BsonClassMap.RegisterClassMap<QueryTemplate>(cm =>
        {
            cm.AutoMap();
            cm.GetMemberMap(c => c.IsMarkedToDelete).SetSerializer(new BooleanOrStringSerializer());
        });
    }

    // THIS IS THE CRITICAL CHANGE: The constructor is now much simpler.
    // It no longer depends on IOptions or IMongoClient. It is GIVEN the
    // exact database it needs to work with.
    public MongoQueryTemplateRepository(
        IMongoDatabase database,
        ILogger logger)
    {
        _templatesCollection = database.GetCollection<QueryTemplate>("QueryTemplates");
        _logger = logger;
    }

    // ALL OTHER METHODS (GetAllAsync, GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync)
    // REMAIN EXACTLY THE SAME. They don't need to change because they already
    // operate on the pre-configured _templatesCollection.

    public async Task<IEnumerable<QueryTemplate>> GetAllAsync()
    {
        _logger.LogInformation("Getting all query templates");
        return await _templatesCollection.Find(_ => true).ToListAsync();
    }

    public async Task<PagedResult<QueryTemplate>> GetPagedAsync(PaginationParameters parameters)
    {
        _logger.LogInformation("Getting paged query templates, Page: {Page}, PageSize: {PageSize}", parameters.Page,
            parameters.PageSize);
        parameters.ValidateAndSetDefaults();

        var filter = Builders<QueryTemplate>.Filter.Empty;
        var totalCount = await _templatesCollection.CountDocumentsAsync(filter);

        var query = _templatesCollection.Find(filter);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            var sortField = GetSortField(parameters.SortBy);
            var sortDefinition = parameters.SortDirection.ToLower() == "desc"
                ? Builders<QueryTemplate>.Sort.Descending(sortField)
                : Builders<QueryTemplate>.Sort.Ascending(sortField);
            query = query.Sort(sortDefinition);
        }

        // Apply pagination
        var skip = (parameters.Page - 1) * parameters.PageSize;
        var items = await query.Skip(skip).Limit(parameters.PageSize).ToListAsync();

        return PagedResult<QueryTemplate>.Create(items, parameters.Page, parameters.PageSize, totalCount);
    }

    public async Task<long> GetCountAsync()
    {
        _logger.LogInformation("Getting count of query templates");
        return await _templatesCollection.CountDocumentsAsync(_ => true);
    }

    public async Task<QueryTemplate?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Getting query template by ID: {TemplateId}", id);
        return await _templatesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(QueryTemplate template)
    {
        _logger.LogInformation("Creating query template with ID: {TemplateId}", template.Id);
        await _templatesCollection.InsertOneAsync(template);
    }

    public async Task<bool> UpdateAsync(QueryTemplate template)
    {
        _logger.LogInformation("Updating query template with ID: {TemplateId}", template.Id);
        var result = await _templatesCollection.ReplaceOneAsync(x => x.Id == template.Id, template);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        _logger.LogInformation("Deleting query template with ID: {TemplateId}", id);
        var result = await _templatesCollection.DeleteOneAsync(x => x.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }

    /// <summary>
    ///     Maps sort field names to MongoDB field paths, including nested fields
    /// </summary>
    private static string GetSortField(string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "id" => "Id",
            "source" => "Template.Source",
            "locale" => "Template.Locale",
            "isdynamicfilter" => "Template.IsDynamicFilter",
            "ismarkedtodelete" => "IsMarkedToDelete",
            "rolesandidsallowedtoread" => "RolesAndIdsAllowedToRead",
            _ => sortBy // Use the field name as-is if no mapping is found
        };
    }
}