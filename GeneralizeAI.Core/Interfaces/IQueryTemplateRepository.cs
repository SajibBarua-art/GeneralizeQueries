using GeneralizeAI.Core.Entities;

namespace GeneralizeAI.Core.Interfaces;
public interface IQueryTemplateRepository
{
    Task<QueryTemplate?> GetByIdAsync(string id);
    Task<IEnumerable<QueryTemplate>> GetAllAsync();
}