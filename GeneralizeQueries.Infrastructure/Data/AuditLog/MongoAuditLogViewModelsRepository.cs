using GeneralizeQueries.Core.DTOs.AuditLog;
using GeneralizeQueries.Core.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GeneralizeQueries.Infrastructure.Data.AuditLog;

public class MongoAuditLogViewModelsRepository : IAuditLogViewModelsRepository
{
    private readonly IMongoCollection<BsonDocument> _collection;
    private readonly ILogger _logger;

    public MongoAuditLogViewModelsRepository(
        IMongoDatabase database,
        string collectionName,
        ILogger logger)
    {
        _collection = database.GetCollection<BsonDocument>(collectionName);
        _logger = logger;
    }

    public async Task<(List<AuditLogViewModel> Items, long TotalCount)> GetAllPagedAsync(
        int page,
        int pageSize,
        string? sortBy,
        string sortDirection,
        string? serviceIdFilter)
    {
        _logger.LogInformation(
            "Getting paged AuditLog entries, Page: {Page}, PageSize: {PageSize}, ServiceIdFilter: {ServiceIdFilter}",
            page, pageSize, serviceIdFilter);

        // Build filter
        var filterBuilder = Builders<BsonDocument>.Filter;
        var filter = filterBuilder.Empty;

        if (!string.IsNullOrWhiteSpace(serviceIdFilter)) filter = filterBuilder.Eq("ServiceId", serviceIdFilter);

        var totalCount = await _collection.CountDocumentsAsync(filter);

        var query = _collection.Find(filter);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            var sortDefinition = sortDirection.ToLower() == "desc"
                ? Builders<BsonDocument>.Sort.Descending(sortBy)
                : Builders<BsonDocument>.Sort.Ascending(sortBy);
            query = query.Sort(sortDefinition);
        }
        else
        {
            // Default sort by LogTime descending
            query = query.Sort(Builders<BsonDocument>.Sort.Descending("LogTime"));
        }

        // Apply pagination
        var skip = (page - 1) * pageSize;
        var documents = await query.Skip(skip).Limit(pageSize).ToListAsync();

        // Map BsonDocument to AuditLogViewModel
        var items = new List<AuditLogViewModel>();
        foreach (var doc in documents)
            try
            {
                var item = new AuditLogViewModel
                {
                    Id = GetGuidValue(doc, "_id"),
                    ServiceId = GetStringValue(doc, "ServiceId"),
                    ItemId = GetStringValue(doc, "ItemId"),
                    Action = GetStringValue(doc, "Action"),
                    UserId = GetGuidValue(doc, "UserId"),
                    ResponseBody = GetStringValue(doc, "ResponseBody"),
                    LogTime = GetDateTimeValue(doc, "LogTime")
                };
                items.Add(item);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to map audit log document, skipping");
            }

        _logger.LogInformation("Retrieved {Count} AuditLog entries out of {TotalCount} total", items.Count, totalCount);
        return (items, totalCount);
    }

    public async Task<AuditLogDetailViewModel?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting AuditLog by ID: {Id}", id);

        var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
        var doc = await _collection.Find(filter).FirstOrDefaultAsync();

        if (doc == null)
        {
            _logger.LogWarning("AuditLog with ID {Id} not found", id);
            return null;
        }

        try
        {
            var item = new AuditLogDetailViewModel
            {
                Id = GetGuidValue(doc, "_id"),
                ServiceId = GetStringValue(doc, "ServiceId"),
                ItemId = GetStringValue(doc, "ItemId"),
                Action = GetStringValue(doc, "Action"),
                UserId = GetGuidValue(doc, "UserId"),
                RequestBody = GetStringValue(doc, "RequestBody"),
                ResponseBody = GetStringValue(doc, "ResponseBody"),
                PayLoad = GetStringValue(doc, "PayLoad"),
                LogTime = GetDateTimeValue(doc, "LogTime")
            };

            _logger.LogInformation("Successfully retrieved AuditLog with ID: {Id}", id);
            return item;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to map audit log document with ID: {Id}", id);
            return null;
        }
    }

    private Guid GetGuidValue(
        BsonDocument doc,
        string fieldName)
    {
        if (!doc.Contains(fieldName) || doc[fieldName].IsBsonNull)
            return Guid.Empty;

        try
        {
            var value = doc[fieldName];

            // Handle BsonBinaryData (CSUUID format)
            if (value.IsBsonBinaryData)
            {
                var binaryData = value.AsBsonBinaryData;

                // Try different approaches to convert CSUUID to Guid
                try
                {
                    // Method 1: Use ToGuid with GuidRepresentation
                    return binaryData.ToGuid(GuidRepresentation.CSharpLegacy);
                }
                catch
                {
                    try
                    {
                        // Method 2: Direct conversion
                        return binaryData.AsGuid;
                    }
                    catch
                    {
                        // Method 3: Manual byte conversion
                        var bytes = binaryData.Bytes;
                        if (bytes != null && bytes.Length == 16) return new Guid(bytes);
                    }
                }
            }

            // Handle string representation
            if (value.IsString)
                if (Guid.TryParse(value.AsString, out var guid))
                    return guid;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to convert field {FieldName} to Guid, BsonType: {BsonType}", fieldName,
                doc.Contains(fieldName) ? doc[fieldName].BsonType : BsonType.Null);
        }

        return Guid.Empty;
    }

    private string? GetStringValue(
        BsonDocument doc,
        string fieldName)
    {
        if (!doc.Contains(fieldName) || doc[fieldName].IsBsonNull)
            return null;

        try
        {
            return doc[fieldName].AsString;
        }
        catch
        {
            return doc[fieldName].ToString();
        }
    }

    private DateTime GetDateTimeValue(
        BsonDocument doc,
        string fieldName)
    {
        if (!doc.Contains(fieldName) || doc[fieldName].IsBsonNull)
            return DateTime.MinValue;

        try
        {
            if (doc[fieldName].IsBsonDateTime)
                return doc[fieldName].ToUniversalTime();
        }
        catch
        {
            // Ignore conversion errors
        }

        return DateTime.MinValue;
    }
}