namespace GeneralizeQueries.Core.Interfaces;

public interface IDynamicMongoRepository
{
    // This method takes connection details as arguments
    Task<List<string>> ListCollectionNamesAsync(string connectionString, string databaseName);
}