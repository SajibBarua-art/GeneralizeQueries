using Platform.Infrastructure.Core.Models;

namespace GeneralizeQueries.Core.Entities.FeatureManagement;

public class FeatureViewModel : ViewModelBase
{
    public string Name { get; set; } = string.Empty;
    public string UniqName { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
    public List<string> Commands { get; set; } = new();
    public List<string>? ReadAuthorizations { get; set; }
    public List<string>? WriteAuthorizations { get; set; }
}