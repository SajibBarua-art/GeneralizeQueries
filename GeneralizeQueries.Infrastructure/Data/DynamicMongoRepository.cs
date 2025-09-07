using GeneralizeQueries.Core.Interfaces;
using MongoDB.Driver;

namespace GeneralizeQueries.Infrastructure.Data;

public class DynamicMongoRepository : IDynamicMongoRepository
{
    public async Task<List<string>> ListCollectionNamesAsync(string connectionString, string databaseName)
    {
        // Create a new client and database connection on-the-fly.
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);

        var collectionNamesCursor = await database.ListCollectionNamesAsync();
        return await collectionNamesCursor.ToListAsync();
    }
}