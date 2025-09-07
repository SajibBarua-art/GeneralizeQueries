using GeneralizeQueries.Core.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GeneralizeQueries.Infrastructure.Data;

public class DynamicQueryRepository : IDynamicQueryRepository
{
    public async Task<List<BsonDocument>> GetAllDocumentsAsync(string connectionString, string databaseName, string collectionName)
    {
        // Create a new client and database connection on-the-fly for this specific request.
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        var collection = database.GetCollection<BsonDocument>(collectionName);

        return await collection.Find(new BsonDocument()).ToListAsync();
    }
}