// GeneralizeAI.Application/GenericDocumentService.cs
using GeneralizeAI.Core.Entities;
using GeneralizeAI.Core.Interfaces;

namespace GeneralizeAI.Application;

public class GenericDocumentService : IGenericDocumentService
{
    private readonly IGenericDocumentRepository _repository;

    public GenericDocumentService(IGenericDocumentRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<GenericDocument>> GetAllDocumentsAsync() => _repository.GetAllAsync();
    public Task<GenericDocument?> GetDocumentByIdAsync(string id) => _repository.GetByIdAsync(id);
    public Task CreateDocumentAsync(GenericDocument document) => _repository.CreateAsync(document);
    
    public async Task<bool> UpdateDocumentAsync(string id, GenericDocument document)
    {
        var existingDoc = await _repository.GetByIdAsync(id);
        if (existingDoc == null)
        {
            return false;
        }
        document.Id = existingDoc.Id; // Ensure the ID is not changed
        return await _repository.UpdateAsync(document);
    }

    public Task<bool> DeleteDocumentAsync(string id) => _repository.DeleteAsync(id);
}