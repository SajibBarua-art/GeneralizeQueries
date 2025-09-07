using GeneralizeQueries.Core.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeneralizeQueries.Infrastructure.Data;

/// <summary>
/// This is the MongoDB Collection Worker. It knows the specific commands to talk to a MongoDB database.
/// It is given the exact database to work with by its factory.
/// </summary>
public class MongoCollectionRepository : ICollectionRepository
{
    private readonly IMongoDatabase _database;

    // The worker is hired and GIVEN access to the factory floor (the specific database instance).
    // It no longer depends on IOptions or IMongoClient.
    public MongoCollectionRepository(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task<IEnumerable<string>> GetCollectionNamesAsync()
    {
        // The worker uses its tools on the assigned factory floor to get the list of collections.
        var cursor = await _database.ListCollectionNamesAsync();
        return await cursor.ToListAsync();
    }

    public async Task<List<string>> GetFieldNamesAsync(string collectionName)
    {
        var collection = _database.GetCollection<BsonDocument>(collectionName);
        var uniqueFieldNames = new HashSet<string>();
        var sampleDocuments = await collection.Find(new BsonDocument()).Limit(10).ToListAsync();

        foreach (var document in sampleDocuments)
        {
            foreach (var field in document.Elements)
            {
                uniqueFieldNames.Add(field.Name);
            }
        }
        
        var sortedFieldNames = uniqueFieldNames.ToList();
        sortedFieldNames.Sort();
        return sortedFieldNames;
    }
}