using GeneralizeAI.Core.Interfaces;

namespace GeneralizeAI.Application;

public class CollectionService
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
}