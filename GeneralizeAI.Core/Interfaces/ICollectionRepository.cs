namespace GeneralizeAI.Core.Interfaces;

public interface ICollectionRepository
{
    Task<IEnumerable<string>> GetCollectionNamesAsync();
}