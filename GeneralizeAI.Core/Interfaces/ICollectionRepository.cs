namespace GeneralizeAI.Core.Interfaces;

public interface ICollectionRepository
{
    Task<IEnumerable<string>> GetCollectionNamesAsync();
    
    /// The worker must know how to get the field names from a given collection in the database.
    Task<List<string>> GetFieldNamesAsync(string collectionName);
}