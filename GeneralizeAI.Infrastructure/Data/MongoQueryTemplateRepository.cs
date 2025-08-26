using GeneralizeAI.Core.Entities;
using GeneralizeAI.Core.Interfaces;
using GeneralizeAI.Infrastructure.Serialization;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace GeneralizeAI.Infrastructure.Data;

public class MongoQueryTemplateRepository : IQueryTemplateRepository
{
    private readonly IMongoCollection<QueryTemplate> _templatesCollection;
    
    static MongoQueryTemplateRepository()
    {
        // This is the fluent mapping API. It tells the driver how to map the class.
        BsonClassMap.RegisterClassMap<TemplateDetails>(cm =>
        {
            cm.AutoMap(); // Map all other properties automatically
            // For this specific member, use our custom serializer
            cm.GetMemberMap(c => c.IsDynamicFilter)
                .SetSerializer(new BooleanOrStringSerializer());
        });
        
        BsonClassMap.RegisterClassMap<QueryTemplate>(cm =>
        {
            cm.AutoMap(); // Map all other properties automatically
            // For this specific member, use our reusable custom serializer
            cm.GetMemberMap(c => c.IsMarkedToDelete)
                .SetSerializer(new BooleanOrStringSerializer());
        });
    }

    public MongoQueryTemplateRepository(IOptions<MongoDbSettings> settings)
    {
        var mongoClient = new MongoClient(settings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);
        // NOTE: We are targeting a NEW collection named "QueryTemplates"
        _templatesCollection = mongoDatabase.GetCollection<QueryTemplate>("QueryTemplates");
    }

    public async Task<IEnumerable<QueryTemplate>> GetAllAsync() =>
        await _templatesCollection.Find(_ => true).ToListAsync();

    public async Task<QueryTemplate?> GetByIdAsync(string id) =>
        await _templatesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    // v-- ADD NEW METHODS --v
    public async Task CreateAsync(QueryTemplate template) =>
        await _templatesCollection.InsertOneAsync(template);

    public async Task<bool> UpdateAsync(QueryTemplate template)
    {
        var result = await _templatesCollection
            .ReplaceOneAsync(x => x.Id == template.Id, template);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _templatesCollection.DeleteOneAsync(x => x.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}