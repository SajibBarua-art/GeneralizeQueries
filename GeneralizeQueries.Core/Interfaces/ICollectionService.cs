namespace GeneralizeQueries.Core.Interfaces;

public interface ICollectionService
{
    /// A task to get all collection names.
    Task<IEnumerable<string>> GetAllCollectionNames();

    /// A task to get the field names from a specific collection.
    Task<List<string>> GetFieldNamesForCollectionAsync(string collectionName);
}