// GeneralizeAI.Core/Interfaces/IGenericDocumentService.cs
using GeneralizeAI.Core.Entities;

namespace GeneralizeAI.Core.Interfaces;

public interface IGenericDocumentService
{
    Task<IEnumerable<GenericDocument>> GetAllDocumentsAsync();
    Task<GenericDocument?> GetDocumentByIdAsync(string id);
    Task CreateDocumentAsync(GenericDocument document);
    Task<bool> UpdateDocumentAsync(string id, GenericDocument document);
    Task<bool> DeleteDocumentAsync(string id);
}