using GeneralizeAI.Core.Interfaces;
using MongoDB.Driver;
using MongoDB.Bson;

namespace GeneralizeAI.Infrastructure.Data;

public class MongoCollectionRepository : ICollectionRepository
{
    private readonly IMongoDatabase _database;

    // We'll get the database connection from the outside (this is called dependency injection).
    public MongoCollectionRepository(IMongoDatabase database)
    {
        _database = database;
    }

    // Here is the actual MongoDB-specific code.
    public async Task<IEnumerable<string>> GetCollectionNamesAsync()
    {
        var collections = await _database.ListCollectionNames().ToListAsync();
        return collections.OrderBy(name => name);
    }
    
    public async Task<List<string>> GetFieldNamesAsync(string collectionName)
    {
        Console.WriteLine($"Repository (Worker): Starting work to find field names in '{collectionName}'.");

        // The worker finds the specific filing cabinet (collection).
        var collection = _database.GetCollection<BsonDocument>(collectionName);

        // The worker prepares a box (HashSet) to put the unique names in.
        var uniqueFieldNames = new HashSet<string>();
        
        Console.WriteLine("Repository (Worker): Grabbing a sample of 100 documents to inspect.");
        
        // The worker grabs a few sample files (documents) to look at, so it doesn't have to read them all.
        var sampleDocuments = await collection.Find(new BsonDocument()).Limit(5).ToListAsync();

        // The worker looks at each file in the sample.
        foreach (var document in sampleDocuments)
        {
            // For each file, it reads the titles of all the sections (the field names).
            foreach (var field in document.Elements)
            {
                // It puts each unique title into its box.
                uniqueFieldNames.Add(field.Name);
            }
        }
        
        Console.WriteLine($"Repository (Worker): Found {uniqueFieldNames.Count} unique field names. Sorting them now.");
        
        // After checking the samples, the worker organizes the titles alphabetically.
        var sortedFieldNames = uniqueFieldNames.ToList();
        sortedFieldNames.Sort();

        // The worker reports back with the final, organized list.
        return sortedFieldNames;
    }
}