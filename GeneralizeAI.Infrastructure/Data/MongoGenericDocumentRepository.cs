// GeneralizeAI.Infrastructure/Data/MongoGenericDocumentRepository.cs
using GeneralizeAI.Core.Entities;
using GeneralizeAI.Core.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GeneralizeAI.Infrastructure.Data;

public class MongoGenericDocumentRepository : IGenericDocumentRepository
{
    private readonly IMongoCollection<GenericDocument> _documentsCollection;

    public MongoGenericDocumentRepository(IOptions<MongoDbSettings> settings)
    {
        var mongoClient = new MongoClient(settings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _documentsCollection = mongoDatabase.GetCollection<GenericDocument>(settings.Value.CollectionName);
    }

    public async Task<IEnumerable<GenericDocument>> GetAllAsync() =>
        await _documentsCollection.Find(_ => true).ToListAsync();

    public async Task<GenericDocument?> GetByIdAsync(string id) =>
        await _documentsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(GenericDocument document) =>
        await _documentsCollection.InsertOneAsync(document);

    public async Task<bool> UpdateAsync(GenericDocument document)
    {
        var result = await _documentsCollection.ReplaceOneAsync(x => x.Id == document.Id, document);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _documentsCollection.DeleteOneAsync(x => x.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}