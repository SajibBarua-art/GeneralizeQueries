namespace GeneralizeQueries.Core.Interfaces;

public interface ICollectionService
{
    /// <summary>
    ///     A task to get all collection names using a specific worker.
    /// </summary>
    Task<IEnumerable<string>> GetAllCollectionNames(ICollectionRepository repository);

    /// <summary>
    ///     A task to get the field names from a specific collection using a specific worker.
    /// </summary>
    Task<List<string>> GetFieldNamesForCollectionAsync(
        ICollectionRepository repository,
        string collectionName);
}