using MongoDB.Bson;

namespace GeneralizeQueries.Core.Interfaces;

public interface IDynamicQueryService
{
    Task<List<BsonDocument>> GetAllDocumentsFromCollectionAsync(string serviceId, string collectionName);
}