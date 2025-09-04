using GeneralizeQueries.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeneralizeQueries.Application;

/// <summary>
/// This is the Collection Manager. Its job is to manage tasks related to collections.
/// It is stateless and performs its tasks using the worker it is given.
/// </summary>
public class CollectionService : ICollectionService
{
    // The service is now stateless and has no dependencies in its constructor.
    public CollectionService() {}

    public async Task<IEnumerable<string>> GetAllCollectionNames(ICollectionRepository repository)
    {
        // It tells the provided worker to do its job.
        Console.WriteLine("Service (Manager): Asking the repository worker for all collection names.");
        return await repository.GetCollectionNamesAsync();
    }
    
    public async Task<List<string>> GetFieldNamesForCollectionAsync(ICollectionRepository repository, string collectionName)
    {
        // The manager receives a request for field names and gives a clear instruction to the provided worker.
        Console.WriteLine($"Service (Manager): Telling the repository worker to get field names for '{collectionName}'.");
        return await repository.GetFieldNamesAsync(collectionName);
    }
}