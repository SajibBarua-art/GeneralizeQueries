using Platform.Infrastructure.Core.Domain;

namespace GeneralizeQueries.Core.Entities.FeatureManagement;

public class FeatureAggregateRoot : AggregateRoot
{
    public List<string>? RolesAndIdsAllowedToRead { get; set; }

    public List<string>? ReadAuthorizations { get; set; }

    public List<string>? WriteAuthorizations { get; set; }

    public string Name { get; set; } = null!;

    public string UniqName { get; set; } = null!;

    public List<string> Commands { get; set; } = [];

    public string TagName { get; set; } = null!;
}