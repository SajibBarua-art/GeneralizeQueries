// GeneralizeAI.Core/Interfaces/IGenericDocumentRepository.cs
using GeneralizeAI.Core.Entities;

namespace GeneralizeAI.Core.Interfaces;

public interface IGenericDocumentRepository
{
    Task<IEnumerable<GenericDocument>> GetAllAsync();
    Task<GenericDocument?> GetByIdAsync(string id);
    Task CreateAsync(GenericDocument document);
    Task<bool> UpdateAsync(GenericDocument document);
    Task<bool> DeleteAsync(string id);
}