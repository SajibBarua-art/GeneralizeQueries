using GeneralizeAI.Core.Entities;

namespace GeneralizeAI.Core.Interfaces;
public interface IQueryTemplateService
{
    Task<QueryTemplate?> GetTemplateByIdAsync(string id);
    Task<IEnumerable<QueryTemplate>> GetAllTemplatesAsync();
    Task CreateTemplateAsync(QueryTemplate template);
    Task<bool> UpdateTemplateAsync(string id, QueryTemplate template);
    Task<bool> DeleteTemplateAsync(string id);
}