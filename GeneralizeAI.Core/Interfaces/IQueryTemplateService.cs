using GeneralizeAI.Core.Entities;

namespace GeneralizeAI.Core.Interfaces;
public interface IQueryTemplateService
{
    Task<QueryTemplate?> GetTemplateByIdAsync(string id);
    Task<IEnumerable<QueryTemplate>> GetAllTemplatesAsync();
}