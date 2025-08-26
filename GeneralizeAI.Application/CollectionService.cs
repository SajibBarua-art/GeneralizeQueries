using GeneralizeAI.Core.Interfaces;

namespace GeneralizeAI.Application;

public class CollectionService: ICollectionService
{
    private readonly ICollectionRepository _collectionRepository;

    // It asks for a worker that fits the job description (ICollectionRepository).
    public CollectionService(ICollectionRepository collectionRepository)
    {
        _collectionRepository = collectionRepository;
    }

    public async Task<IEnumerable<string>> GetAllCollectionNames()
    {
        // It tells the worker to do its job.
        Console.WriteLine("Service: Asking the repository for collection names.");
        return await _collectionRepository.GetCollectionNamesAsync();
    }
    
    public async Task<List<string>> GetFieldNamesForCollectionAsync(string collectionName)
    {
        // The manager receives a request for field names and gives a clear instruction to the worker.
        Console.WriteLine($"Service (Manager): Telling the repository worker to get field names for '{collectionName}'.");
        return await _collectionRepository.GetFieldNamesAsync(collectionName);
    }
}