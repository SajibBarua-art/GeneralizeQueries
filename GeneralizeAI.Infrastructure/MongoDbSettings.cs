// GeneralizeAI.Infrastructure/MongoDbSettings.cs
namespace GeneralizeAI.Infrastructure;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string CollectionName { get; set; } = null!;
}