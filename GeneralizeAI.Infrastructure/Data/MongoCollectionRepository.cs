using GeneralizeAI.Core.Interfaces;
using MongoDB.Driver;

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
        var cursor = await _database.ListCollectionNamesAsync();
        return await cursor.ToListAsync();
    }
}