using GeneralizeQueries.Core.Entities;
using GeneralizeQueries.Core.Interfaces;
using GeneralizeQueries.Infrastructure.Serialization;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace GeneralizeQueries.Infrastructure.Data;

public class MongoQueryTemplateRepository : IQueryTemplateRepository
{
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
    public MongoQueryTemplateRepository(IMongoDatabase database)
    {
        _templatesCollection = database.GetCollection<QueryTemplate>("QueryTemplatesTest");
    }

    // ALL OTHER METHODS (GetAllAsync, GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync)
    // REMAIN EXACTLY THE SAME. They don't need to change because they already
    // operate on the pre-configured _templatesCollection.

    public async Task<IEnumerable<QueryTemplate>> GetAllAsync() =>
        await _templatesCollection.Find(_ => true).ToListAsync();

    public async Task<QueryTemplate?> GetByIdAsync(string id) =>
        await _templatesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    
    public async Task CreateAsync(QueryTemplate template) =>
        await _templatesCollection.InsertOneAsync(template);

    public async Task<bool> UpdateAsync(QueryTemplate template)
    {
        var result = await _templatesCollection.ReplaceOneAsync(x => x.Id == template.Id, template);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _templatesCollection.DeleteOneAsync(x => x.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}