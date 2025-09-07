using MongoDB.Bson;

namespace GeneralizeQueries.Core.Interfaces;

// This repository's methods take connection details as parameters.
public interface IDynamicQueryRepository
{
    Task<List<BsonDocument>> GetAllDocumentsAsync(string connectionString, string databaseName, string collectionName);
}