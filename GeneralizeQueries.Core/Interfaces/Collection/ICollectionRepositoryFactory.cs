namespace GeneralizeQueries.Core.Interfaces;

/// <summary>
///     Defines the contract for a factory that creates ICollectionRepository instances.
///     This allows for creating repositories configured for specific databases on-the-fly.
/// </summary>
public interface ICollectionRepositoryFactory
{
    /// <summary>
    ///     Creates a repository instance based on a service ID lookup.
    /// </summary>
    /// <param name="serviceId">The ID of the service from ServiceRegistrations.json.</param>
    /// <returns>A configured repository instance, or null if the serviceId is not found.</returns>
    Task<ICollectionRepository?> CreateRepositoryAsync(string serviceId);
}